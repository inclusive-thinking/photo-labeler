# PhotoLabeler

## ¿Qué es PhotoLabeler?

Photo Labeler es un pequeño programa para renombrar fotografías y vídeos basándose en los metadatos que éstas contengan.
Está desarrollado con .Net Core y Electron, por lo que debería ser compatible con Windows, Mac y Linux.

## Instalación

### Para Windows

PhotoLabeler es portable, por lo que solo tendrás que descargar el fichero .Zip, descomprimirlo en alguna carpeta en la que tengas permisos de escritura, y ejecutar la aplicación PhotoLabeler.exe.

### Para MAC

Descarga el fichero .dmg, monta el volumen, y copia el paquete .app a tu directorio de aplicaciones. Una vez copiado, ejecuta la aplicación desde ahí.

## Funcionamiento

El funcionamiento es muy sencillo. La aplicación consta de dos botones: "Seleccionar carpeta" y "Salir".

Además, tiene dos menús: archivo (llamado PhotoLabeler en Mac), e Idioma. En el menú de idioma podrás elegir en qué idioma quieres ver la aplicación, actualmente solo se soportan inglés y español.

###  Seleccionar carpeta

Al pulsar el botón "Seleccionar carpeta", se abrirá un diálogo del sistema para que elijas el directorio sobre el que deseas empezar a trabajar.

**¡Nota importante!** Si utilizas MAC, durante las pruebas de usuario se ha detectado que, al pulsar el botón, aunque el diálogo del sistema se abre, VoiceOver no es consciente de esta apertura, y deberéis reiniciarlo sin cambiar de ventana para empezar a interactuar con el diálogo. Ya hay una issue abierta en el repositorio de Electron hablando sobre este tema: [Electron Issue: Dialog box message not read out by screen reader](https://github.com/electron/electron/issues/14234). Estaré pendiente a esta issue para actualizar la documentación en cuanto esté resuelta.

Una vez hayas elegido el directorio, verás dos controles: una presentación en árbol con los subdirectorios del directorio abierto, y a continuación, una tabla con las fotos del directorio seleccionado. En esa tabla, verás una columna llamada "Etiqueta", en el que aparecerá la etiqueta de la fotografía seleccionada.
Al tabular, verás una casilla de verificación que te permite ocultar aquellas fotografías que no tengan etiqueta, y tras esta casilla de verificación, un botón que te permitirá renombrar todas las fotografías que tengan etiqueta, para que el nombre del archivo coincida con dicho nombre.
### Renombrar fotografías
Al pulsar este botón, el sistema nos preguntará si deseamos renombrar las fotos etiquetadas, o nos informará de que no hay fotos para etiquetar. En caso de que haya fotos para renombrar y pulsemos el botón "sí", las fotos se renombrarán, y el sistema nos avisará cuando finalice la operación.

Las fotografías quedarán renombradas con un prefijo numérico para mantener el orden de fecha de creación ascendente. En futuras versiones, esta opción será configurable.

 ## Salir
 
 Este botón te permite salir de la aplicación.


 ## ¿Cómo etiquetar fotografías en el iPhone y renombrarlas con PhotoLabeler
 
 1. Abre la aplicación "Fotos.
 2. Ve al álbum que desees etiquetar.
 3. Elige la foto que quieras etiquetar, y pulsa dos veces con dos dedos, manteniendo los dedos en la pantalla en la segunda pulsación. Sonarán cuatro pitidos, y en el cuarto, se abrirá un diálogo para eteiquetar el elemento. Este diálogo se utiliza para etiquetar no solo fotografías, sino también cualquier elemento que queramos etiquetar dentro de cualquier aplicación. Lo bueno de etiquetar las fotografías, es que la etiqueta no solo se guarda en VoiceOver, sino que, al exportar esa fotografía, la etiqueta queda adjunta a los metadatos de la foto. Son estos metadatos los que utiliza PhotoLabeler para renombrar los nombres de los ficheros (que no son nada descriptivos) para que coincidan con dicha eetiqueta.
 4. Una vez etiquetada, entra en la foto, y pulsa en el botón "Compartir". Elige la aplicación que sdsees para exportar la fotografía. En MAC, lo más sencillo es exportarla con Airdrop, y en Windows o LInux, podéis utilizar aplicaciones de almacenamiento como Google Drive o Mega. Dropbox también se puede utilizar, pero hasta donde yo sé, si seleccionamos varias fotografías a la vez, la opción no aparece en el menú de compartir.
 5. Una vez tengas  las fotos en tu  ordenador, utiliza la opción de "Abrir carpeta" de PhotoLabeler para acceder al directorio donde estén dichas fotos, y a continuación, pulsa el botón "Renombrar fotos". Desde ese momento, todas las fotos del directorio tendrán nombres descriptivos, basados en la etiqueta que previamente hayas puesto en el iPhone.
 
 ## Contribuyendo al desarrollo de PhotoLabeler
 
 Si deseas contribuir al desarrollo de PhotoLabeler, por favor, lee el fichero "contributing.md" de este mismo directorio.
 