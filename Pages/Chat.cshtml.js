// This is the JavaScript file for the Chat page.
class ChatHistory {
    /** 種類(assistant, user) @type {string} */
    type;
    /** コンテンツ @type {string} */
    content;
    /**
     * @param {string} type 種類(assistant, user)
     * @param {string} content コンテンツ
     */
    constructor(type, content) {
        this.type = type;
        this.content = content;
    }
}
class ChatManager {

    /** チャット内容 @type {Array<ChatHistory>} */
    #histories = [];
    /** 待機メッセージの要素 @type {HTMLElement|null} */
    #waitingMessageElement = null;
    /** 待機メッセージのタイマーID @type {number|null} */
    #waitingMessageTimerId = null;

    constructor() {
        this.messageInput = document.getElementById('messageInput');
        this.sendMessageButton = document.getElementById('sendMessageButton');
        this.chatContainer = document.getElementById('chat-container');
        this.setupEventListeners();
    }

    setupEventListeners() {
        this.sendMessageButton.addEventListener('click', () => this.sendMessage());
    }

    /**
     * 待機メッセージを表示し、タイマーを開始します。
     */
    #showWaitingMessage() {
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
        const startTime = Date.now();

        this.#waitingMessageTimerId = setInterval(() => {
            const elapsedTime = (Date.now() - startTime) / 1000;
            this.#waitingMessageElement.textContent = `応答を待っています...${elapsedTime.toFixed(1)}s`;
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
            this.#waitingMessageElement.closest('.chat-message').remove();
            this.#waitingMessageElement = null;
        }
    }

    #appendMessage(messageText, type, index) {
        const messageDiv = document.createElement('div');
        messageDiv.dataset.index = index;
        messageDiv.setAttribute("id", "message-" + index);
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
                historyItem.content = chatContent.innerText;
                chatContent.innerHTML = marked.parse(historyItem.content);
                editButton.textContent = '✏️';
            } else {
                // Enter edit mode
                chatContent.classList.add('form-control');
                chatContent.setAttribute('contentEditable', 'plaintext-only');
                chatContent.innerText = historyItem.content;
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

            // Remove from DOM
            messageDiv.remove();

            // Remove from histories array
            this.#histories.splice(indexToRemove, 1);

            // Re-synchronize dataset.index for remaining messages
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

        document.location.hash = "message-" + index;
    }

    async #regenerateMessage(indexToRegenerate) {
        // 待機メッセージを表示
        this.#showWaitingMessage();

        try {
            // 再生成するメッセージより前の履歴を取得
            const historiesForRegeneration = this.#histories.slice(0, indexToRegenerate);

            const response = await fetch('/api/Chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(historiesForRegeneration)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const responseData = await response.json();
            const newAssistantMessage = responseData[responseData.length - 1];

            if (newAssistantMessage && newAssistantMessage.type === 'assistant') {
                // 既存のメッセージを置き換える
                this.#histories[indexToRegenerate].content = newAssistantMessage.content;
                const messageElement = document.getElementById(`message-${indexToRegenerate}`);
                if (messageElement) {
                    const chatContent = messageElement.querySelector('.chat-content');
                    chatContent.innerHTML = marked.parse(newAssistantMessage.content);
                }
                document.location.hash = `message-${indexToRegenerate}`;
            }
        } catch (error) {
            console.error('Error regenerating message:', error);
            // エラーメッセージを表示するか、適切なハンドリングを行う
            alert(`メッセージの再生成中にエラーが発生しました: ${error.message}`);
        } finally {
            this.#hideWaitingMessage(); // 待機メッセージを非表示
        }
    }

    async sendMessage() {
        const messageText = this.messageInput.value.trim();
        if (messageText) {
            await this.userMessage(messageText);
            this.messageInput.value = ''; // Clear input

            this.#showWaitingMessage(); // Display waiting message

            try {
                const response = await fetch('/api/Chat', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.#histories)
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const responseData = await response.json();
                // The API returns the entire updated list of messages, so the last one is the new assistant message.
                const lastMessage = responseData[responseData.length - 1];
                if (lastMessage && lastMessage.type === 'assistant') {
                    this.#hideWaitingMessage(); // Hide waiting message
                    await this.assistantMessage(lastMessage.content);
                }
            } catch (error) {
                this.#hideWaitingMessage(); // Hide waiting message on error
                console.error('Error sending message:', error);
                await this.assistantMessage(`エラーが発生しました: ${error.message}`);
            }
        }
    }

    async assistantMessage(messageText) {
        this.#appendMessage(messageText, 'assistant', this.#histories.length);
        this.#histories.push(new ChatHistory("assistant", messageText));
    }

    async userMessage(messageText) {
        this.#appendMessage(messageText, 'user', this.#histories.length);
        this.#histories.push(new ChatHistory("user", messageText));
    }
}

document.addEventListener('DOMContentLoaded', async function () {
    const chat = new ChatManager();
    await chat.assistantMessage("こんにちは！何かお手伝いできることはありますか？");
});
