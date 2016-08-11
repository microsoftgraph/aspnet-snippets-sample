/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

function onSelectedItemChanged() {
    $('.needs-id').map(function () {
        if (this.name !== 'not-supported') {
            var selectedItem = $('input[name="items"]:checked');
            var id = selectedItem.val();
            if (id) {

                // Enable the button, and set the item ID in the hidden id field.
                $('#' + this.id).prop('disabled', false);
                $('.selected-id').prop('value', id);

                // Set the item name in the hidden name field, if it exists.
                if ($('.selected-name')) {
                    var displayName = selectedItem[0].previousSibling.parentNode.innerText;
                    $('.selected-name').prop('value', displayName.substring(2));
                }
            }
            else $('#' + this.id).prop('disabled', true);
        }
    });
}