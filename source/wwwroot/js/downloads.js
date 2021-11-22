const productSelectorSelector = '#product-selector';
const downloadsTableSelector = "#tblDownloads";

$(document).ready(function () {
    $(productSelectorSelector).change(onDownloadProductChanged);
    onDownloadProductChanged();
});

// Called when the product dropdown on the downloads page is changed.
// Updates the displayed product versions.
function onDownloadProductChanged() {
    var product = $(productSelectorSelector).find('option:selected').text();
    $(`${downloadsTableSelector} tr`).each(function() {
        if (this.className == product) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
    $(`${downloadsTableSelector} tr:first`).show();
}
