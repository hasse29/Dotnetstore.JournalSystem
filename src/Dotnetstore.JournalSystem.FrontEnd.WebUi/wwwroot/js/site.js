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
