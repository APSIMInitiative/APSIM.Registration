const productSelectorSelector = '#product-selector';
const versionRowSelector = '#versionRow';
const apsimName = "APSIM Next Generation";
const oldApsimName = "APSIM Classic";
const apsoilName = "Apsoil";
const commercialLicenceName = "Commercial";
const nonCommercialLicenceName = "Non-Commercial";

$(document).ready(function () {
    $('#dropDownLicenseType').change(onLicenseTypeChanged);
    $('#Product').change(onProductChanged);
    $(productSelectorSelector).change(onDownloadProductChanged);
    onLicenseTypeChanged();
    onProductChanged();
    onDownloadProductChanged();
});

function onLicenseTypeChanged() {
    // The options for commercial users should only be visible
    // when the commercial license type radio button is checked.
    var dropDown = $('#dropDownLicenseType')[0];
    if (dropDown != null) {
        if (dropDown.selectedIndex == 1) {
            $('.commercialInput').each(function () {
                $(this).show();
            });
        } else {
            $('.commercialInput').each(function () {
                $(this).hide();
            });
        }
        updateLicense();
    }
}

function onDownloadProductChanged() {
    var product = $(productSelectorSelector).find('option:selected').text();
    $('#tblDownloads tr').each(function() {
        if (this.className == product) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
    $('#tblDownloads tr:first').show();
}


// This is called when the user changes the selected product on the
// registration page.
function onProductChanged() {
    // Need to update license info and hide the row containing version
    // selection if apsoil is selected.
    updateLicense();
    var product = $('#Product').find('option:selected').text();

    // If user has selected apsim (ng), show the Linux/MacOS platforms.
    // Otherwise, hide them.
    if (product == apsimName) {
        $('#platformLinux').show();
        $('#platformMacOS').show();
    } else {
        $('#platformLinux').hide();
        $('#platformMacOS').hide();
        $('#platform-selector').prop('selectedIndex', 0);
    }

    // If user has selected old apsim, show the version dropdown.
    // (This allows them to select historical major releases such as 7.9).
    // Otherwise, hide the version selector.
    if (product == oldApsimName) {
        $(versionRowSelector).show();
    } else {
        $(versionRowSelector).hide();
        $(versionRowSelector).prop('selectedIndex', 0);
    }
}

function updateLicense() {
    var product = $('#Product').find('option:selected').text();
    var src = '';
    if (product == apsimName || product == oldApsimName) {
        var licenceType = $('#dropDownLicenseType').find('option:selected').text()
        if (licenceType == nonCommercialLicenceName)
            src = 'APSIM_NonCommercial_RD_licence.htm';
        else {
            src = 'APSIM_Commercial_Licence.htm';
        }
    } else {
        src = 'OtherDisclaimer.html';
    }
    $('#terms').prop('src', `/html/${src}`);
}
