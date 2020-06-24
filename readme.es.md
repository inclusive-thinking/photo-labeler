# Photo Labeler

## ¿Qué es Photo Labeler?

Photo Labeler es un pequeño programa para renombrar fotografías y vídeos basándose en los metadatos que éstas contengan.
	Está desarrollado con .Net Core y Electron, por lo que debería ser compatible con Windows, Mac y Linux.

## Instalación

Photo Labeler es portable, por lo que solo tendrás que descargar el fichero .Zip de tu sistema operativo, descomprimirlo en alguna carpeta en la que tengas permisos de escritura, y ejecutar la aplicación PhotoLabeler.

## Funcionamiento

E funcionamiento es muy sencillo. La aplicación consta de dos botones: "Seleccionar carpeta" y "Salir".

###  Seleccionar carpeta

Al pulsar el botón "Seleccionar carpeta", se abrirá un diálogo del sistema para que elijas el directorio sobre el que deseas empezar a trabajar.
**¡Nota importante!** Si utilizas MAC, durante las pruebas de usuario se ha detectado que, al pulsar el botón, aunque el diálogo del sistema se abre, VoiceOver no es consciente de esta apertura, y deberéis reiniciarlo sin cambiar de ventana para empezar a interactuar con el diálogo. Ya hay una issue abierta en el repositorio de Electron hablando sobre este tema: https://github.com/electron/electron/issues/14234
Estaré pendiente a esta issue para actualizar la documentación en cuanto esté resuelta.
