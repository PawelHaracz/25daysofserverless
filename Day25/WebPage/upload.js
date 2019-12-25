const url = 'http://localhost:7071/api';

$(document).ready(function () {
  const form = document.querySelector('#target');

  form.addEventListener('submit', e => {
    e.preventDefault();

    const files = document.querySelector('[type=file]').files;
    const formData = new FormData();

    for (let i = 0; i < files.length; i++) {
      let file = files[i];

      formData.append('files[]', file);
    }

    fetch(`${url}/call`, {
      method: 'POST',
      body: formData,
      mode: 'no-cors'
    }).then(response => {
      console.log(response);
    })
  })

  $(".custom-file-input").on("change", function () {
    var fileName = $(this).val().split("\\").pop();
    $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
    let files = document.querySelector('[type=file]').files;
    if (files && files[0]) {
      var reader = new FileReader();
      reader.onload = function (e) {
        $('#preview').attr('src', e.target.result);
      }
      reader.readAsDataURL(this.files[0]);
    }
  });
});

