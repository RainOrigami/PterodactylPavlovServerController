// Small UI helpers the Blazor components call into.
window.ppscUi = window.ppscUi || {};

// Opens a Bootstrap modal that is already in the DOM. Most confirmations are
// wired declaratively with data-bs-toggle, but a form that has to validate
// first can only decide to ask once its handler has run.
window.ppscUi.showModal = function (id) {
    var element = document.getElementById(id);
    if (!element || typeof bootstrap === "undefined") {
        return false;
    }

    bootstrap.Modal.getOrCreateInstance(element).show();

    return true;
};
