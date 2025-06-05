document.addEventListener('DOMContentLoaded', function () {
    // アクセストークンのコピーボタン
    document.querySelectorAll('.copy-token-button').forEach(button => {
        button.addEventListener('click', function () {
            const tokenValue = this.dataset.token;
            navigator.clipboard.writeText(tokenValue).then(() => {
                const successMessage = this.closest('td').querySelector('.copy-success-message');
                if (successMessage) {
                    successMessage.style.display = 'inline';
                    setTimeout(() => {
                        successMessage.style.display = 'none';
                    }, 2000);
                }
            }).catch(err => {
                console.error('Failed to copy token: ', err);
            });
        });
    });

    // SSO URLのコピーボタン
    document.querySelectorAll('.copy-sso-url-button').forEach(button => {
        button.addEventListener('click', function () {
            const ssoUrl = this.dataset.url;
            navigator.clipboard.writeText(ssoUrl).then(() => {
                const successMessage = this.closest('.input-group').nextElementSibling;
                if (successMessage && successMessage.classList.contains('copy-sso-success-message')) {
                    successMessage.style.display = 'inline';
                    setTimeout(() => {
                        successMessage.style.display = 'none';
                    }, 2000);
                }
            }).catch(err => {
                console.error('Failed to copy SSO URL: ', err);
            });
        });
    });
});
