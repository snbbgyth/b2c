$(document).ready(function () {
    $("#bar-notification").delay(5000).fadeOut(400);
});
    /*Custom Drop Down*/
$(document.body).on('click', '.dropdown-menu li', function(event){
    var $target = $(event.currentTarget);
    $target.closest('.btn-group')
       .find('[data-bind="label"]').text( $target.text())
          .end()
       .children('.dropdown-toggle').dropdown('toggle');
    return false;
});



        

