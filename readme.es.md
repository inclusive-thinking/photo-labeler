# PhotoLabeler

## ¿Qué es PhotoLabeler?

Photo Labeler es un pequeño programa para renombrar fotografías y vídeos basándose en los metadatos que éstas contengan.
Está desarrollado con .Net Core y Electron, por lo que debería ser compatible con Windows, Mac y Linux.

## Construyendo el proyecto

Si quieres contribuir con PhotoLabeler, lo primero de todo, ¡muchas gracias! :)

### Prerrequisitos

Photo Labeler está construido utilizando .Net Core 3.1 como una aplicación Blazor server y Electron .Net.
#### Entorno de desarrollo

Para poder compilar el proyecto y comenzar a trabajar con él, necesitarás, o bien Visual Studio, o Visual Studio Code.
* Windows:
 * Visual Studio: [Visual Studio Community 2019](https://visualstudio.microsoft.com/vs/community/), con la carga de trabajo de ".Net Core": [Configurar Visual Studio para trabajar con .Net Core](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=netcore31#install-with-visual-studio).
 * Visual studio Code: [Última versión de Visual Studio Code](https://code.visualstudio.com/download). Para trabajar con .Net Core, consulta [cómo configurar Visual Studio Code para trabajar con .Net Core](https://code.visualstudio.com/docs/languages/dotnet).
* Mac:
 * [Visual Studio 2019 For Mac](https://visualstudio.microsoft.com/en/vs/mac/), con la carga de trabajo de .Net Core: [Configurar Visual Studio 2019 for Mac con .Net Core](https://visualstudio.microsoft.com/vs/mac/net/).
 * Visual studio Code: [Última versión de Visual Studio Code](https://code.visualstudio.com/download). Para trabajar con .Net Core, consulta [cómo configurar Visual Studio Code para trabajar con .Net Core](https://code.visualstudio.com/docs/languages/dotnet).
* Linux:
 * Visual studio Code: [Última versión de Visual Studio Code](https://code.visualstudio.com/download). Para trabajar con .Net Core, consulta [cómo configurar Visual Studio Code para trabajar con .Net Core](https://code.visualstudio.com/docs/languages/dotnet).

#### Electron y Electron.Net

Una vez instalado y configurado el entorno de desarrollo, necesitarás instalar todo lo necesario para ejecutar electron en tu máquina junto con .Net Core.

* Instala el gestor de paquetes NPM, contenido en [NodeJS](https://nodejs.org/en/)
* Instala  la [CLI de Electron.Net](https://www.nuget.org/packages/ElectronNET.CLI/). Revisa las instrucciones de instalación en la página de nuget.

### Descargando el repositorio

1. Ve al repositorio en : [https://github.com/inclusive-thinking/photo-labeler](https://github.com/inclusive-thinking/photo-labeler).
2. Inicia sesión en Github con tu usuario, si aún no lo has hecho.
3. Pulsa en el botón "Fork" para crear una copia del repositorio bajo tu propio usuario.
4. Una vez hecho el fork, clona el repositorio que acabas de crear:
```bash
git clone https://github.com/tu_usuario/photo-labeler
```
5. Entra al repositorio
```bash
cd photo-labeler
```
6. photo-labeler depende de una librería de terceros llamada metadata-extractor-dotnet. Esta librería está añadida como submódulo, por lo que tendrémos que inicializarlo:
```bash
git submodule update --init --recursive
```
7. Accede al directorio src/PhotoLabeler, y en la terminal, ejecuta el comando:
```bash
electronize start
```
8. Tras unos segundos, debería abrirse la ventana de la aplicación y podrás empezar a utilizarla. Si deseas depurar la aplicación utilizando Visual Studio o Visual Studio Code, adjúntate al proceso "PhotoLabeler" que deberá aparecer en la lista de procesos. Aquí tienes un artículo sobre cómo [depurar aplicaciones .Net Core con Visual Studio Code](https://medium.com/@mikezrimsek/debugging-dotnet-core-projects-with-visual-studio-code-ff0ab66ecc70).

### ¿Cómo contribuir?

Lo más fácil es que vayas a la página de [issues del proyecto](https://github.com/inclusive-thinking/photo-labeler/issues). Ahí, podrás ver las issues que hay abiertas y colaborar en alguna de ellas en la que aún no esté trabajando nadie. También puedes crear tu propia issue si encuentras un error o propones una nueva característica. En estos dos últimos casos, sería prudente esperar a que un miembro del proyecto te confirmara que efectivamente el error existe, o que la característica que quieres añadir es coherente con el proyecto.
1. accede a la issue y mira el número que tiene asignado (está en la cabecera, en el título y al final de la URL).
2. Sincroniza la rama develop de tu repositorio con la rama develop del repositorio padre (el de inclusive-thinking/photo labeler). [Aquí tienes una guía de cómo hacerlo](https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/syncing-a-fork).
3. Crea una rama basada en la recién sincronizada rama develop llamada feature/[issue_number]-proposito-muy-corto-separado-por-guiones. Por ejemplo: "feature/47-add-focus-indicator".
```bash
git checkout -b feature/47-add-focus-indicator
```
Si es un bug, en lugar de feature, antepón el nombre de la rama con el prefijo "bugfix": bugfix/48-fix-typo-in-main-menu.
4. Añade un comentario en la issue, indicando que estás trabajando en ella. Si ya has subido tu nueva rama a tu repositorio remoto, sería interesante que añadieras el enlace a esa rama, por si alguien quiere ir revisando el progreso del trabajo.
5. Trabaja en tu rama. Si tu trabajo dura algunos días, sería bueno que volvieses a sincronizar la rama de develop con los últimos cambios del repositorio padre, e hicieses un merge de la rama develop actualizada a tu rama. Mientras más actualizada esté la rama con la que trabajas con la rama de develop del repositorio padre, menos conflictos habrá cuando crees la "pull request" para integrar tus cambios.
6. En la sección de navegación de tu repositorio, haz click en "Pull requests" y a continuación, en "Create pull request".
7. Compara tu rama con la rama develop del repositorio padre. Si todo ha ido bien, te debería informar de que las ramas se pueden unir.
8. Haz click en el botón "Create pull requests", y rellena los campos tal y como se te piden.
9. En la descripción, vincula tu PR con la issue en la que estabas trabajando. [Aquí tienes una guía de cómo enlazar issues con pull requests](https://docs.github.com/en/enterprise/2.17/user/github/managing-your-work-on-github/linking-a-pull-request-to-an-issue). Por ejemplo: Fixes inclusive-thinking/photo-labeler#47
10. Una vez finalices de rellenar los campos, completa la creación de la pull request. Se te notificará por correo de los cambios de la misma, incluyendo los comentarios que los revisores te hagan sobre ella.
11. Una vez tu PR sea aprobada y completada, tus cambios estarán disponibles en la rama develop y la issue vinculada será cerrada automaticamente. ¡Buen trabajo! :)


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
3. Elige la foto que quieras etiquetar.
3.1. Si utilizas  Voice Over: Pulsa dos veces con dos dedos, manteniendo los dedos en la pantalla en la segunda pulsación. Sonarán cuatro pitidos, y en el cuarto, se abrirá un diálogo para eteiquetar el elemento. Este diálogo se utiliza para etiquetar no solo fotografías, sino también cualquier elemento que queramos etiquetar dentro de cualquier aplicación. Lo bueno de etiquetar las fotografías, es que la etiqueta no solo se guarda en VoiceOver, sino que, al exportar esa fotografía, la etiqueta queda adjunta a los metadatos de la foto. Son estos metadatos los que utiliza PhotoLabeler para renombrar los nombres de los ficheros (que no son nada descriptivos) para que coincidan con dicha eetiqueta.
3.2. Si utilizas iOS 14, desliza con un dedo hacia arriba sobre la foto, y verás que te aparece un campo para añadir un pie de foto. Escribe la etiqueta y pulsa en Aceptar.
3.3. Si no usas VoiceOver ni tienes iOS 14, no podrás etiquetar las fotos.
4. Una vez etiquetada, entra en la foto, y pulsa en el botón "Compartir". Elige la aplicación que dessees para exportar la fotografía. En MAC, lo más sencillo es exportarla con Airdrop, y en Windows o LInux, podéis utilizar aplicaciones de almacenamiento como Google Drive o Mega. Dropbox también se puede utilizar, pero hasta donde yo sé, si seleccionamos varias fotografías a la vez, la opción no aparece en el menú de compartir.
5. Una vez tengas  las fotos en tu  ordenador, utiliza la opción de "Abrir carpeta" de PhotoLabeler para acceder al directorio donde estén dichas fotos, y a continuación, pulsa el botón "Renombrar fotos". Desde ese momento, todas las fotos del directorio tendrán nombres descriptivos, basados en la etiqueta que previamente hayas puesto en el iPhone.

