const url = 'http://localhost:7071/api';

$(document).ready(function () {
  const form = $('#target');
  const video = $('#video');
  const canvas = $('#canvas');
  const context = canvas[0].getContext('2d');
  const snap = $("#snap");
  const retake = $("#retake");
  const submit = $("#submit");

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(url, signalR.HttpTransportType.ServerSentEvents)
    .configureLogging(signalR.LogLevel.Information)
    .build();

  connection.on('broadcastMessage', procesedMessage);
  connection.onclose(() => console.log('disconnected'));
  console.log('connecting...');
  connection.start()
    .then(console.log)
    .catch(console.error);

  retakePhoto();
  if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
    navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
      video[0].srcObject = stream;
      video[0].play();
    });
  }

  snap.on("click", function () {
    context.drawImage(video[0], 0, 0, 640, 480);
    video.hide();
    canvas.show();
    retake.show();
    submit.show();
    snap.hide();
  });

  retake.on("click", retakePhoto);

  form.submit(e => {
    e.preventDefault();

    canvas[0].toBlob(blob => {
      const formData = new FormData();

      formData.append('files[]', blob)

      fetch(`${url}/Gift`, {
        method: 'POST',
        body: formData,
        mode: 'cors'
      })
        .then(console.log)
        .catch(console.error);
    })
  });

  function procesedMessage(message) {
    alert(`Your packed gift is ${message.Result ? "" : "not"} ok`);
  }

  function retakePhoto() {
    video.show();
    canvas.hide();
    retake.hide();
    submit.hide();
    snap.show();
  }
});



