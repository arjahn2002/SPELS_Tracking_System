// Sidebar functionalities
$(document).on('click', '.toggle', function () {
    $('.sidebar').toggleClass('close');
});

$(document).ready(function () {
    // Initialize tooltips
    const tooltipTriggers = document.querySelectorAll('#UserMngBtn, #MngAccBtn, #UserRoleBtn, #SubmitBtn, #OtherFOsBtn, #GenderBtn, #SEBtn, #ComplianceBtn, #StatusBtn, #ProvinceBtn, #PositionBtn');

    tooltipTriggers.forEach(trigger => {
        new bootstrap.Tooltip(trigger, {
            boundary: 'window',   // <- set boundary to window
            placement: 'right',   // <- explicitly set placement
            fallbackPlacements: ['right', 'bottom', 'top', 'left'], // allow fallback
            offset: [0, 8]        // <- move it slightly away from the button
        });
    });

    function toggleTooltips() {
        tooltipTriggers.forEach(trigger => {
            let tooltipInstance = bootstrap.Tooltip.getInstance(trigger);
            if ($('.sidebar').hasClass('close')) {
                tooltipInstance?.enable();
            } else {
                tooltipInstance?.disable();
            }
        });
    }

    // Listen for sidebar transition
    $('.sidebar').on('transitionend', function () {
        toggleTooltips();
    });

    // Initial check
    toggleTooltips();
});