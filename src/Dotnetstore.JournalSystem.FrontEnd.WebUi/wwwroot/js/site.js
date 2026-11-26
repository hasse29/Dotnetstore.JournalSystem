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
                var first = dialog.querySelector("input:not([type=hidden]), select, textarea");
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

    // Oversized files are refused here so that they are never uploaded; the server checks the size again.
    document.addEventListener("change", function (event) {
        var input = event.target;
        if (!input.matches || !input.matches("input[type=file][data-max-bytes]")) {
            return;
        }

        var file = input.files && input.files[0];
        var tooLarge = !!file && file.size > Number(input.getAttribute("data-max-bytes"));
        input.setCustomValidity(tooLarge ? "The file is too large." : "");
        input.reportValidity();
    });

    document.addEventListener("submit", function (event) {
        var message = event.target.getAttribute && event.target.getAttribute("data-confirm");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });
})();

(function () {
    "use strict";

    // Ticking a right ticks the ones it needs; unticking one unticks those that need it. The API adds them too.
    document.addEventListener("change", function (event) {
        var box = event.target;
        var group = box.closest && box.closest("[data-permissions]");
        if (!group || box.type !== "checkbox") {
            return;
        }

        var boxes = Array.prototype.slice.call(group.querySelectorAll("input[type=checkbox]"));
        var requires = function (item) {
            return (item.getAttribute("data-requires") || "").split(" ").filter(Boolean);
        };
        var pending = [box];
        while (pending.length) {
            var current = pending.pop();
            boxes.forEach(function (other) {
                var related = box.checked
                    ? requires(current).indexOf(other.value) >= 0
                    : requires(other).indexOf(current.value) >= 0;
                if (related && other.checked !== box.checked) {
                    other.checked = box.checked;
                    pending.push(other);
                }
            });
        }
    });
})();
