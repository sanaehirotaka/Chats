
const regenerateRandomButton = document.querySelector('#regenerateRandomButton');
const regenerateRandomWaitingMessage = document.querySelector('#regenerateRandomWaitingMessage');
async function regenerateRandom(request) {
    const response = await fetch('/api/CharaGenerator', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({request})
    });
    if (response.ok) {
        const data = await response.json();
        document.querySelector('#Input_Name').value = data.charactorName;
        document.querySelector('#Input_SystemPrompt').value = data.systemPrompt;
    } else {
        console.error(response);
    }
}
regenerateRandomButton.addEventListener('click', async function () {
    regenerateRandomButton.classList.add('d-none');

    const request = await new Prompt("キャラクターを生成するにあたり要望があれば自由に入力してください", "要望を入力してください", "").show();

    const startTime = Date.now();
    const waitingMessageTimerId = setInterval(() => {
        const elapsedTime = (Date.now() - startTime) / 1000;
        regenerateRandomWaitingMessage.textContent = `応答を待っています...${elapsedTime.toFixed(1)}s`;
    }, 100);

    try {
        await regenerateRandom(request);
        await regenerateDescription();
    } catch (error) {
        console.error('Error:', error);
        alert('通信エラーが発生しました。');
    } finally {
        clearInterval(waitingMessageTimerId);
        regenerateRandomWaitingMessage.replaceChildren('');
        regenerateRandomButton.classList.remove('d-none');
    }
});

const regenerateButton = document.querySelector('#regenerateDescriptionButton');
const regenerateDescriptionWaitingMessage = document.querySelector('#regenerateDescriptionWaitingMessage');
async function regenerateDescription() {
    const payload = {
        messages: [new ChatHistory('user', 'あなたの自己紹介を200文字以内で提示してください。')],
        systemPrompt: document.querySelector('#Input_SystemPrompt').value
    };
    const response = await fetch('/api/Chat/Talk', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    });

    if (response.ok) {
        const data = await response.json();
        if (Array.isArray(data)) {
            document.querySelector('#Input_Description').value = data[data.length - 1].content;
        } else if (data.error) {
            alert('エラー: ' + data.error);
        }
    } else {
        console.error(response);
    }
}

regenerateButton.addEventListener('click', async function () {
    regenerateButton.classList.add('d-none');

    const startTime = Date.now();
    const waitingMessageTimerId = setInterval(() => {
        const elapsedTime = (Date.now() - startTime) / 1000;
        regenerateDescriptionWaitingMessage.textContent = `応答を待っています...${elapsedTime.toFixed(1)}s`;
    }, 100);

    try {
        await regenerateDescription();

    } catch (error) {
        console.error('Error:', error);
        alert('通信エラーが発生しました。');
    } finally {
        clearInterval(waitingMessageTimerId);
        regenerateDescriptionWaitingMessage.replaceChildren('');
        regenerateButton.classList.remove('d-none');
    }
});