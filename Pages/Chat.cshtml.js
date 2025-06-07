
class ChatManager {

    /** チャット内容 @type {Array<ChatHistory>} */
    #histories = [];
    /** 待機メッセージの要素 @type {HTMLElement|null} */
    #waitingMessageElement = null;
    /** 待機メッセージのタイマーID @type {number|null} */
    #waitingMessageTimerId = null;
    /** システムプロンプト @type {string|undefined} */
    systemPrompt = undefined;
    /** 応答を待っています... */
    waitingMessage = "応答を待っています...";

    constructor() {
        this.messageInput = document.getElementById('messageInput');
        this.sendMessageButton = document.getElementById('sendMessageButton');
        this.chatContainer = document.getElementById('chat-container');
        this.candicateContainer = document.querySelector("#candicate-container");
        this.modelSelect = document.getElementById('modelSelect'); // Add this line
        this.setupEventListeners();
    }

    setupEventListeners() {
        this.sendMessageButton.addEventListener('click', () => this.sendInputMessage());

        this.candicateContainer.addEventListener('click', e => {
            if (e.target.classList.contains("btn")) {
                this.sendMessage(e.target.textContent);
            }
        });
    }

    /**
     * 待機メッセージを表示し、タイマーを開始します。
     */
    #showWaitingMessage(contentElement = undefined) {

        this.candicateContainer.replaceChildren();

        if (contentElement) {
            this.#waitingMessageElement = contentElement;
        } else {
            const messageDiv = document.createElement('div');
            messageDiv.classList.add('chat-message', 'waiting');

            const messageBubble = document.createElement('div');
            messageBubble.classList.add('message-bubble');
            messageDiv.append(messageBubble);

            const chatContent = document.createElement('div');
            chatContent.classList.add('chat-content');
            messageBubble.append(chatContent);

            const actionButtons = document.createElement("div");
            actionButtons.classList.add('chat-action');
            messageBubble.append(actionButtons);

            this.chatContainer.append(messageDiv);
            this.chatContainer.scrollTop = this.chatContainer.scrollHeight;

            this.#waitingMessageElement = chatContent;
        }
        const startTime = Date.now();

        this.#waitingMessageTimerId = setInterval(() => {
            const elapsedTime = (Date.now() - startTime) / 1000;
            this.#waitingMessageElement.textContent = `${this.waitingMessage}${elapsedTime.toFixed(1)}s`;
        }, 100);
    }

    /**
     * 待機メッセージを非表示にし、タイマーを停止します。
     */
    #hideWaitingMessage() {
        if (this.#waitingMessageTimerId) {
            clearInterval(this.#waitingMessageTimerId);
            this.#waitingMessageTimerId = null;
        }
        if (this.#waitingMessageElement) {
            this.#waitingMessageElement.closest('.chat-message.waiting')?.remove();
            this.#waitingMessageElement = null;
        }
    }

    #appendMessage(messageText, type, index) {
        const messageDiv = document.createElement('div');
        messageDiv.dataset.index = index;
        messageDiv.setAttribute("id", "_" + index);
        messageDiv.classList.add('chat-message', type);

        const messageBubble = document.createElement('div');
        messageBubble.classList.add('message-bubble');
        messageBubble.tabIndex = 0;

        const chatContent = document.createElement('div');
        chatContent.classList.add('chat-content');
        chatContent.innerHTML = marked.parse(messageText);
        messageBubble.append(chatContent);

        const actionButtons = document.createElement("div");
        actionButtons.classList.add('chat-action');
        messageBubble.append(actionButtons);

        const editButton = document.createElement('button');
        editButton.classList.add('chat-action-button', 'edit-button');
        editButton.append('✏️'); // ペンの絵文字に変更
        actionButtons.append(editButton);

        editButton.addEventListener('click', () => {
            const index = parseInt(messageDiv.dataset.index);
            const historyItem = this.#histories[index];

            if (chatContent.hasAttribute('contentEditable')) {
                // Exit edit mode
                chatContent.classList.remove('form-control');
                chatContent.removeAttribute('contentEditable');
                historyItem.content = chatContent.textContent;
                chatContent.innerHTML = marked.parse(historyItem.content);
                editButton.textContent = '✏️';
            } else {
                // Enter edit mode
                chatContent.classList.add('form-control');
                chatContent.setAttribute('contentEditable', 'plaintext-only');
                chatContent.textContent = historyItem.content;
                chatContent.focus();
                editButton.textContent = '✅'; // チェックマークの絵文字に変更
            }
        });

        const deleteButton = document.createElement('button');
        deleteButton.classList.add('chat-action-button', 'delete-button');
        deleteButton.append('🗑️'); // ゴミ箱の絵文字
        actionButtons.append(deleteButton);

        deleteButton.addEventListener('click', () => {
            const indexToRemove = parseInt(messageDiv.dataset.index);

            messageDiv.remove();

            this.#histories.splice(indexToRemove, 1);

            this.chatContainer.querySelectorAll('.chat-message').forEach((msgDiv, newIndex) => {
                msgDiv.dataset.index = newIndex;
            });
        });

        if (type === 'assistant') {
            const regenerateButton = document.createElement('button');
            regenerateButton.classList.add('chat-action-button', 'regenerate-button');
            regenerateButton.append('🔄'); // 再生成の絵文字
            actionButtons.append(regenerateButton);

            regenerateButton.addEventListener('click', () => {
                const indexToRegenerate = parseInt(messageDiv.dataset.index);
                this.#regenerateMessage(indexToRegenerate);
            });
        }
        messageDiv.append(messageBubble);
        this.chatContainer.append(messageDiv);
        document.location.hash = "_" + index;
    }

    async #regenerateMessage(indexToRegenerate) {
        // 待機メッセージを表示
        this.#showWaitingMessage(document.querySelector(`#_${indexToRegenerate} .chat-content`));

        try {
            // 再生成するメッセージより前の履歴を取得
            const historiesForRegeneration = this.#histories.slice(0, indexToRegenerate);

            const [provider, model] = this.modelSelect.value.split("/", 2);
            const payload = {
                messages: historiesForRegeneration,
                provider: provider,
                model: model,
                systemPrompt: this.systemPrompt
            };

            const response = await fetch('/api/Chat/Talk', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const responseData = await response.json();
            const newAssistantMessage = responseData[responseData.length - 1];

            if (newAssistantMessage && newAssistantMessage.type === 'assistant') {
                // 既存のメッセージを置き換える
                this.#histories[indexToRegenerate].content = newAssistantMessage.content;
                const messageElement = document.getElementById(`_${indexToRegenerate}`);
                if (messageElement) {
                    const chatContent = messageElement.querySelector('.chat-content');
                    chatContent.innerHTML = marked.parse(newAssistantMessage.content);
                }
                document.location.hash = `_${indexToRegenerate}`;
            }
        } catch (error) {
            console.error('Error regenerating message:', error);
            // エラーメッセージを表示するか、適切なハンドリングを行う
            alert(`メッセージの再生成中にエラーが発生しました: ${error.message}`);
        } finally {
            this.#hideWaitingMessage(); // 待機メッセージを非表示
        }
    }

    async sendInputMessage() {
        const messageText = this.messageInput.value.trim();
        if (messageText) {
            this.messageInput.value = ''; // Clear input
            await this.sendMessage(messageText);
        }
    }

    async sendMessage(messageText) {
        if (!messageText && this.#histories.length != 0 && this.systemPrompt == undefined) {
            return;
        }

        if (messageText) {
            await this.userMessage(messageText);
        }

        this.#showWaitingMessage(); // Display waiting message

        try {
            const [provider, model] = this.modelSelect.value.split("/", 2);
            const payload = {
                messages: this.#histories,
                provider: provider,
                model: model,
                systemPrompt: this.systemPrompt
            };

            const response = await fetch('/api/Chat/Talk', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload) // Send payload
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const responseData = await response.json();
            const lastMessage = responseData[responseData.length - 1];
            if (lastMessage && lastMessage.type === 'assistant') {
                this.#hideWaitingMessage();
                await this.assistantMessage(lastMessage.content);
            }
        } catch (error) {
            this.#hideWaitingMessage();
            console.error('Error sending message:', error);
            await this.assistantMessage(`エラーが発生しました: ${error.message}`);
        }
    }

    async requestCandicate() {
        const [provider, model] = this.modelSelect.value.split("/", 2);
        const payload = {
            messages: this.#histories,
            provider: provider,
            model: model
        };
        const response = await fetch('/api/Chat/ActionCandicate', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const responseData = await response.json();

        this.candicateContainer.replaceChildren();

        for (const text of responseData) {
            const button = document.createElement("a");
            button.classList.add("btn", "btn-sm", "btn-primary");
            button.append(text);
            this.candicateContainer.append(button);

        }
    }

    async assistantMessage(messageText) {
        this.#appendMessage(messageText, 'assistant', this.#histories.length);
        this.#histories.push(new ChatHistory("assistant", messageText));
        await this.requestCandicate();
    }

    async userMessage(messageText) {
        this.#appendMessage(messageText, 'user', this.#histories.length);
        this.#histories.push(new ChatHistory("user", messageText));
    }
}

document.addEventListener('DOMContentLoaded', async function () {
    const chat = new ChatManager();
    if (window.AiPersonality) {
        chat.systemPrompt = window.AiPersonality.systemPrompt;
        chat.waitingMessage = window.AiPersonality.name + "が入力しています...";
    }

    if (!chat.systemPrompt) {
        await chat.assistantMessage("こんにちは！何かお手伝いできることはありますか？");
    } else {
        await chat.sendMessage();
    }

    console.log(chat);
});
