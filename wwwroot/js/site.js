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

class Alert {
    constructor(message, title = '通知') {
        this.message = message;
        this.title = title;
        this.customDialogTemplate = document.getElementById('customDialogTemplate');
    }

    show() {
        return new Promise((resolve) => {
            const dialogClone = this.customDialogTemplate.content.cloneNode(true);
            const customDialog = dialogClone.querySelector('dialog');
            const customDialogTitle = customDialog.querySelector('[data-dialog-part="title"]');
            const customDialogMessage = customDialog.querySelector('[data-dialog-part="message"]');
            const customDialogCancelButton = customDialog.querySelector('[data-dialog-part="cancelButton"]');
            const customDialogConfirmButton = customDialog.querySelector('[data-dialog-part="confirmButton"]');
            const customDialogCloseButton = customDialog.querySelector('[data-dialog-part="closeButton"]');

            customDialogTitle.textContent = this.title;
            customDialogMessage.textContent = this.message;
            customDialogCancelButton.style.display = 'none'; // Hide cancel button for alert
            customDialogConfirmButton.textContent = 'OK';

            const closeAndResolve = () => {
                resolve();
                customDialog.close();
                customDialog.remove(); // Remove the dialog from DOM
            };

            customDialogConfirmButton.addEventListener('click', closeAndResolve);
            customDialogCloseButton.addEventListener('click', closeAndResolve);

            // Close dialog when clicking outside of it
            customDialog.addEventListener('click', (event) => {
                if (event.target === customDialog) {
                    closeAndResolve();
                }
            });

            document.body.append(dialogClone);
            customDialog.showModal(); // Show as a modal
        });
    }
}

class Confirm {
    constructor(message, title = '確認') {
        this.message = message;
        this.title = title;
        this.customDialogTemplate = document.getElementById('customDialogTemplate');
    }

    show() {
        return new Promise((resolve) => {
            const dialogClone = this.customDialogTemplate.content.cloneNode(true);
            const customDialog = dialogClone.querySelector('dialog');
            const customDialogTitle = customDialog.querySelector('[data-dialog-part="title"]');
            const customDialogMessage = customDialog.querySelector('[data-dialog-part="message"]');
            const customDialogCancelButton = customDialog.querySelector('[data-dialog-part="cancelButton"]');
            const customDialogConfirmButton = customDialog.querySelector('[data-dialog-part="confirmButton"]');
            const customDialogCloseButton = customDialog.querySelector('[data-dialog-part="closeButton"]');

            customDialogTitle.textContent = this.title;
            customDialogMessage.textContent = this.message;
            customDialogCancelButton.style.display = 'inline-block'; // Show cancel button for confirm
            customDialogConfirmButton.textContent = 'OK';

            const onConfirm = () => {
                resolve(true);
                customDialog.close();
                customDialog.remove(); // Remove the dialog from DOM
            };

            const onCancel = () => {
                resolve(false);
                customDialog.close();
                customDialog.remove(); // Remove the dialog from DOM
            };

            customDialogConfirmButton.addEventListener('click', onConfirm);
            customDialogCancelButton.addEventListener('click', onCancel);
            customDialogCloseButton.addEventListener('click', onCancel); // Close button acts as cancel

            // Close dialog when clicking outside of it
            customDialog.addEventListener('click', (event) => {
                if (event.target === customDialog) {
                    onCancel(); // Clicking outside acts as cancel
                }
            });

            document.body.append(dialogClone);
            customDialog.showModal(); // Show as a modal
        });
    }
}

class Prompt {
    constructor(message, title = '入力', defaultValue = '') {
        this.message = message;
        this.title = title;
        this.defaultValue = defaultValue;
        this.customDialogTemplate = document.getElementById('customDialogTemplate');
    }

    show() {
        return new Promise((resolve) => {
            const dialogClone = this.customDialogTemplate.content.cloneNode(true);
            const customDialog = dialogClone.querySelector('dialog');
            const customDialogTitle = customDialog.querySelector('[data-dialog-part="title"]');
            const customDialogMessage = customDialog.querySelector('[data-dialog-part="message"]');
            const customDialogInput = customDialog.querySelector('[data-dialog-part="input"]');
            const customDialogCancelButton = customDialog.querySelector('[data-dialog-part="cancelButton"]');
            const customDialogConfirmButton = customDialog.querySelector('[data-dialog-part="confirmButton"]');
            const customDialogCloseButton = customDialog.querySelector('[data-dialog-part="closeButton"]');

            customDialogTitle.textContent = this.title;
            customDialogMessage.textContent = this.message;
            
            customDialogInput.style.display = 'block';
            customDialogInput.value = this.defaultValue;

            const onConfirm = () => {
                resolve(customDialogInput.value);
                customDialog.close();
                customDialog.remove();
            };

            const onCancel = () => {
                resolve(null);
                customDialog.close();
                customDialog.remove();
            };

            customDialogConfirmButton.addEventListener('click', onConfirm);
            customDialogCancelButton.addEventListener('click', onCancel);
            customDialogCloseButton.addEventListener('click', onCancel);

            customDialog.addEventListener('click', (event) => {
                if (event.target === customDialog) {
                    onCancel();
                }
            });

            document.body.append(dialogClone);
            customDialog.showModal();
            customDialogInput.focus();
        });
    }
}
