
$(document).ready(function () {
    var table = new DataTable('#TableIndex', {
        paging: true,
        scrollCollapse: true,
        scrollY: '200px'
    });

    $('.dt-search input').attr('placeholder', 'Search here...');
    $('label[for="dt-length-0"]').text('entries');


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

    $("form").on("submit", function () {
        const form = this;

        // Check HTML5 validation
        if (!form.checkValidity()) {
            return;
        }

        // Find the specific button that was clicked
        const $btn = $(form).find(".btn-save");
        const $spinner = $btn.find(".loading-spinner");

        // Disable and show spinner
        $btn.prop("disabled", true).addClass("disabled");
        $spinner.removeClass("d-none");
    });

});

function exportExcel() {
    const fromDate = $("#fromDate").val();
    const toDate = $("#toDate").val();
    const fileName = $("#fileName").val().trim();

    // Input validation
    if (!fromDate || !toDate || !fileName) {
        return showToast("Please fill in all fields.", "warning");
    }

    if (new Date(fromDate) > new Date(toDate)) {
        return showToast("Invalid date range. 'From' date cannot be later than 'To' date.", "warning");
    }

    toggleLoading(true);

    $.ajax({
        url: "/Documents/ExportToExcel",
        type: "GET",
        data: { fromDate, toDate, fileName },
        xhrFields: { responseType: "blob" },
        success(data, status, xhr) {
            toggleLoading(false);

            if (xhr.status === 200) {
                const blob = new Blob([data], { type: xhr.getResponseHeader("Content-Type") });

                if (blob.size === 0) {
                    return showToast("The exported file is empty.", "warning");
                }

                const downloadUrl = URL.createObjectURL(blob);
                const a = $("<a>")
                    .attr({ href: downloadUrl, download: `${fileName}.xlsx` })
                    .appendTo("body");

                a[0].click();
                a.remove();

                showToast("Export successful! Your file is downloading.", "success");
                $("#ExportExcelModal").modal("hide");
            } else {
                showToast("Unexpected error occurred.", "danger");
            }
        },
        error(xhr) {
            toggleLoading(false);
            const message = xhr.status === 400
                ? xhr.responseText || "No records found within the selected date range."
                : "An unexpected error occurred.";
            showToast(message, "danger");
        }
    });
}

function toggleLoading(isLoading) {
    $("#loadingSpinner").toggleClass("d-none", !isLoading);
    $("#btnExport").toggleClass("disabled", isLoading);
}

function showToast(message, type) {
    var toastContainer = $(".toast-container");
    var toastHTML = `<div class="toast align-items-center text-bg-${type} border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
                                <div class="d-flex">
                                    <div class="toast-body">
                                        ${message}
                                    </div>
                                    <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                                </div>
                             </div>`;

    toastContainer.append(toastHTML);
    var toastElement = toastContainer.find(".toast").last();
    var toast = new bootstrap.Toast(toastElement[0]);
    toast.show();

    // Automatically remove toast after it disappears
    toastElement.on("hidden.bs.toast", function () {
        $(this).remove();
    });
}

