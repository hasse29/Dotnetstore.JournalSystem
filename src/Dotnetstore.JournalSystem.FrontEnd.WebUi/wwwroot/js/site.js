(function () {
    "use strict";

    var toggle = document.getElementById("sidebar-toggle");
    if (!toggle) {
        return;
    }

    function setOpen(open) {
        document.body.classList.toggle("sidebar-open", open);
        toggle.setAttribute("aria-expanded", open ? "true" : "false");
    }

    toggle.addEventListener("click", function () {
        setOpen(!document.body.classList.contains("sidebar-open"));
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            setOpen(false);
        }
    });
})();

(function () {
    "use strict";

    document.addEventListener("click", function (event) {
        var opener = event.target.closest("[data-dialog-open]");
        if (opener) {
            var dialog = document.getElementById(opener.getAttribute("data-dialog-open"));
            if (dialog && typeof dialog.showModal === "function") {
                dialog.showModal();
                var first = dialog.querySelector("input, select, textarea");
                if (first) {
                    first.focus();
                }
            }
            return;
        }

        if (event.target.closest("[data-dialog-close]")) {
            var owner = event.target.closest("dialog");
            if (owner) {
                owner.close();
            }
            return;
        }

        if (event.target instanceof HTMLDialogElement && event.target.classList.contains("modal")) {
            event.target.close();
        }
    });

    document.addEventListener("submit", function (event) {
        var message = event.target.getAttribute && event.target.getAttribute("data-confirm");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });
})();
