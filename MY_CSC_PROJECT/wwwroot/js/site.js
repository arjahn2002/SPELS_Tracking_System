
$(document).ready(function () {
    var table = new DataTable('#TableIndex');

    $('.dt-input').attr('placeholder', 'Search here...');
    $('label[for="dt-length-0"]').text('entries');

    // Initialize tooltips for multiple buttons
    const tooltipTriggers = document.querySelectorAll('#UserMngBtn, #MngAccBtn, #UserRoleBtn, #SubmitBtn, #OtherFOsBtn, #GenderBtn, #SEBtn, #ComplianceBtn, #StatusBtn, #ProvinceBtn, #PositionBtn');

    tooltipTriggers.forEach(trigger => {
        new bootstrap.Tooltip(trigger, {
            boundary: document.body
        });
    });

    // Toggle tooltip when sidebar is opened/closed
    $('.sidebar').on('transitionend', function () {
        tooltipTriggers.forEach(trigger => {
            let tooltipInstance = bootstrap.Tooltip.getInstance(trigger);
            if ($('.sidebar').hasClass('close')) {
                tooltipInstance.enable();
            } else {
                tooltipInstance.disable();
            }
        });
    });

    // Initial check on page load
    tooltipTriggers.forEach(trigger => {
        let tooltipInstance = bootstrap.Tooltip.getInstance(trigger);
        if ($('.sidebar').hasClass('close')) {
            tooltipInstance.enable();
        } else {
            tooltipInstance.disable();
        }
    });


    $(".modal-content").draggable({ handle: ".modal-header" });
    $('.modal').on('hidden.bs.modal', function () {
        $(this).find(".modal-content").css({
            top: "",
            left: "",
            transform: "translate(0, 0)"
        });
    });

    function updateDateTime() {
        const now = new Date();

        // Get formatted date (Month Name, Day, Year)
        const dateOptions = { month: 'short', day: 'numeric', year: 'numeric' };
        const formattedDate = now.toLocaleDateString('en-US', dateOptions);

        // Get formatted time (HH:MM:SS AM/PM)
        const timeOptions = { hour: 'numeric', minute: '2-digit', second: '2-digit', hour12: true };
        const formattedTime = now.toLocaleTimeString('en-US', timeOptions);

        // Update the HTML content
        document.getElementById("dateTime").textContent = `${formattedDate} - ${formattedTime}`;
    }

    // Update immediately and refresh every second
    setInterval(updateDateTime, 1000);
    updateDateTime();
});

function exportExcel() {
    var fromDate = $("#fromDate").val();
    var toDate = $("#toDate").val();
    var fileName = $("#fileName").val().trim();

    // Validate input fields
    if (!fromDate || !toDate || !fileName) {
        showToast("Please fill in all fields.", "warning");
        return;
    }

    // Ensure fromDate is not greater than toDate
    if (new Date(fromDate) > new Date(toDate)) {
        showToast("Invalid date range. 'From' date cannot be later than 'To' date.", "warning");
        return;
    }

    $("#loadingSpinner").removeClass("d-none");
    $("#btnExport").addClass("disabled");

    $.ajax({
        url: "/Documents/ExportToExcel",
        type: "GET",
        data: { fromDate, toDate, fileName },
        xhrFields: {
            responseType: "blob"
        },
        success: function (data, status, xhr) {
            $("#loadingSpinner").addClass("d-none");
            $("#btnExport").removeClass("disabled");

            if (xhr.status === 200) {
                var blob = new Blob([data], { type: xhr.getResponseHeader("Content-Type") });

                if (blob.size === 0) {
                    showToast("The exported file is empty.", "warning");
                    return;
                }

                var downloadUrl = window.URL.createObjectURL(blob);
                var a = document.createElement("a");
                a.href = downloadUrl;
                a.download = `${fileName}.xlsx`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);

                showToast("Export successful! Your file is downloading.", "success");
                $("#ExportExcelModal").modal("hide");
            } else {
                showToast("Unexpected error occurred.", "danger");
            }
        },
        error: function (xhr) {
            $("#loadingSpinner").addClass("d-none");
            $("#btnExport").removeClass("disabled");

            if (xhr.status === 400) {
                showToast(xhr.responseText || "No records found within the selected date range.", "danger");
            }
            else {
                showToast("An unexpected error occurred.", "danger");
            }
        }
    });
}