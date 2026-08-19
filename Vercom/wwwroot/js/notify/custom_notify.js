$(document).ready(function () {
  const connection = $.connection.notificacionesHub;
  // Maneja las notificaciones enviadas desde el servidor

  $(".dropdown-menu").on("scroll", function () {
    const container = $(this);
    if (
      container.scrollTop() + container.innerHeight() >=
      container[0].scrollHeight - 10
    ) {
      loadMoreNotifications(); // Cargar más datos automáticamente
    }
  });

  $(document).on("click", "#toggleDark", function () {
    $("body").toggleClass("dark-mode");
    document.cookie =
      "modoOscuro=" + $("body").hasClass("dark-mode") + "; path=/";
  });

  connection.client.recibirNotificacion = function (notificacion) {
    const notificationList = $("#notification-list");
    const notificationCount = $("#notification-count");
    let count = parseInt(notificationCount.text()) || 0;

    // Agregar el mensaje de notificación a la lista
    const fechaConvertida = notificacion.CreatedAt.split(".")[0];
    const fechaInicio = new Date(fechaConvertida);
    const fechaFinTexto =
      '@System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")';
    const fechaFin = new Date(fechaFinTexto);
    const diferencia = fechaFin.getTime() - fechaInicio.getTime();
    const dias = Math.floor(diferencia / (1000 * 60 * 60 * 24));
    const horas = Math.floor(
      (diferencia % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
    );
    const minutos = Math.floor((diferencia % (1000 * 60 * 60)) / (1000 * 60));
    const item = `<li class="notificacion" id="notificacion-${notificacion.Id}">
        <a href="#"><div>${notificacion.Message}<br>
            <button onclick="marcarComoLeidaNotify(${notificacion.Id})" class="btn btn-primary btn-xs" type="button">
                <i class="fa fa-check"></i>&nbsp;Marcar como leída</button>
            <span class="float-right text-muted small">Hace ${dias}d ${horas}h ${minutos}m</span>
        </div></a></li>`;
    notificationList.append(item);
    notificationCount.text(count + 1);
    notificationCount.show();
  };

  function loadNotificaciones() {
    $.get(
      "/Notificaciones/GetUnreadNotifications?username=" +
        "@User.Identity.Name",
      function (data) {
        const notificationList = $("#notification-list");
        const notificationCount = $("#notification-count");
        notificationList.empty();
        notificationCount.empty();
        // Mostrar solo las primeras 5 notificaciones
        const maxNotificaciones = 5;
        const notificacionesVisibles = data.slice(0, maxNotificaciones);
        // Mostrar botón "Ver todas las notificaciones" si hay más de 10
        const verTodas = `<li><div class="text-center link-block"><a href="/Notificaciones/Index">
                      <strong>Ver todas las notificaciones</strong>
                      <i class="fa fa-angle-right"></i></a></div></li>`;
        notificationList.append(verTodas);
        // Procesar las notificaciones visibles
        notificacionesVisibles.forEach((notificacion) => {
          const fechaInicio = new Date(notificacion.CreatedAt);
          const fechaFinTexto =
            '@System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")';
          const fechaFin = new Date(fechaFinTexto);
          const diferencia = fechaFin.getTime() - fechaInicio.getTime();
          const dias = Math.floor(diferencia / (1000 * 60 * 60 * 24));
          const horas = Math.floor(
            (diferencia % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
          );
          const minutos = Math.floor(
            (diferencia % (1000 * 60 * 60)) / (1000 * 60)
          );
          const item = `<li class="notificacion" id="notificacion-${notificacion.Id}">
                          <a href="#"><div>${notificacion.Message}<br>
                          <button onclick="marcarComoLeidaNotify(${notificacion.Id})" class="btn btn-primary btn-xs" type="button">
                          <i class="fa fa-check"></i>&nbsp;Marcar como leída</button>
                          <span class="float-right text-muted small">Hace ${dias}d ${horas}h ${minutos}m</span>
                          </div></a></li>`;
          notificationList.append(item);
        });
        // Actualizar el contador de notificaciones
        notificationCount.text(data.length).show();
      }
    );
  }

  function loadStockNotificaciones() {
    var lastId = 0;
    $.get(
      "/Notificaciones/GetStockNotifications?username=" + "@User.Identity.Name",
      function (data) {
        const notification_stock = $("#notification-stock");
        const notificationstock_Count = $("#notificationsctock-count");
        notification_stock.empty();
        notificationstock_Count.empty();
        // Set a limit of 5 notifications to display
        const maxNotificaciones = 5;
        const notificacionesVisibles = data.slice(0, maxNotificaciones);
        // Process and append visible notifications
        const verTodas = `<li><div class="text-center link-block">
        <a href="/Notificaciones/Index">
            <strong>Ver todas las notificaciones</strong>
            <i class="fa fa-angle-right"></i></a>
    </div></li>`;
        notification_stock.append(verTodas);
        notificacionesVisibles.forEach((notificacion) => {
          const fechaInicio = new Date(notificacion.CreatedAt);
          const fechaFinTexto =
            '@System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")';
          const fechaFin = new Date(fechaFinTexto);
          const diferencia = fechaFin.getTime() - fechaInicio.getTime();
          const dias = Math.floor(diferencia / (1000 * 60 * 60 * 24));
          const horas = Math.floor(
            (diferencia % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
          );
          const minutos = Math.floor(
            (diferencia % (1000 * 60 * 60)) / (1000 * 60)
          );
          const item = `<li class="notificacion" id="notificacion-${notificacion.Id}">
        <a href="#"><div>${notificacion.Message}<br>
            <button onclick="marcarComoLeidaStock(${notificacion.Id})" class="btn btn-primary btn-xs" type="button">
                <i class="fa fa-check"></i>&nbsp;Marcar como leída</button>
            <span class="float-right text-muted small">Hace ${dias}d ${horas}h ${minutos}m</span>
        </div></a></li>`;
          lastId = notificacion.Id;
          notification_stock.append(item);
        });

        // Update the notification count
        notificationstock_Count.text(data.length).show();
      }
    );
  }

  let page = 1; // Variable para rastrear la página actual
  const pageSize = 5; // Número de notificaciones por página

  setInterval(loadNotificaciones(), 30000); // Ejecuta cada 30 segundos

  setInterval(loadStockNotificaciones(), 30000); // Ejecuta cada 30 segundos

  // Iniciar la conexión con SignalR
  $.connection.hub
    .start()
    .done(function () {
      console.log("Conexión establecida con SignalR.");
    })
    .fail(function (error) {
      console.error("Error al conectar con SignalR:", error);
    });
});
