const apsimName = "APSIM Next Generation";
const oldApsimName = "APSIM Classic";
const commercialLicenceName = "Commercial";
const nonCommercialLicenceName = "Non-Commercial";

$(document).ready(function () {
    $('#dropDownLicenseType').change(onLicenseTypeChanged);
    $('#Product').change(onProductChanged);
    onLicenseTypeChanged();
    onProductChanged();
});

// Called when the licence-type dropdown on the registration page
// is changed. Updates the contents of the licence iframe and also
// hides/shows any inputs which are specific to that licence type.
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

// This is called when the user changes the selected product on the
// registration page. Shows/hides the platform dropdown, depending
// on whether the selected product supports multiple platforms.
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
    $('#version-selector option').each(function() { $(this).hide(); });
    $('#version-selector option.latest-version').each(function() { $(this).show(); });
    var productClass = product.replaceAll(' ', '');
    $(`#version-selector option.${productClass}`).each(function() {
        $(this).show();
    });
}

// Update the licence text depending on the selected product and
// licence type.
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
