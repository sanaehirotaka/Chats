const resetPasswordButton = document.getElementById('resetPasswordButton');
const passwordDisplayArea = document.getElementById('passwordDisplayArea');
const generatedPasswordInput = document.getElementById('generatedPassword');
const copyPasswordButton = document.getElementById('copyPasswordButton');

resetPasswordButton.addEventListener('click', async function () {
    const userId = this.dataset.userId;
    const confirm = new Confirm("本当にパスワードをリセットしますか？この操作は元に戻せません。");
    const result = await confirm.show();
    if (result) {
        // Get the anti-forgery token from the form
        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

        try {
            const response = await fetch(`?handler=ResetPassword&id=${userId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': antiForgeryToken,
                    'Content-Type': 'application/json' // Specify content type for JSON
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();

            if (data.success) {
                generatedPasswordInput.value = data.newPassword;
                passwordDisplayArea.style.display = 'block';
                const alert = new Alert("パスワードがリセットされました。");
                await alert.show();
            } else {
                const alert = new Alert("パスワードのリセットに失敗しました: " + (data.errors ? data.errors.join(', ') : "不明なエラー"));
                await alert.show();
            }
        } catch (error) {
            console.error('Error:', error);
            const alert = new Alert('パスワードのリセット中にエラーが発生しました。');
            await alert.show();
        }
    }
});

copyPasswordButton.addEventListener('click', async function () {
    generatedPasswordInput.select();
    generatedPasswordInput.setSelectionRange(0, 99999);
});