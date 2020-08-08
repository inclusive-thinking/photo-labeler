# PhotoLabeler

## What is PhotoLabeler?

Photo Labeler is a small program to rename photos and videos based on their metadata.
It is developed with .Net Core and Electron, so it should be compatible with Windows, Mac and Linux.

## Building the project

If you want to contribute to PhotoLabeler, first of all, thank you very much! :)

### Prerequisites

Photo Labeler is built using .Net Core 3.1 as a Blazor server and Electron .Net application.

#### Development Environment

In order to compile the project and start working with it, you will need either Visual Studio or Visual Studio Code.
* Windows:
 * Visual Studio: [Visual Studio Community 2019](https://visualstudio.microsoft.com/vs/community/), with the ".Net Core" workload: [Configure Visual Studio to work with .Net Core](https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=netcore31#install-with-visual-studio).
 * Visual studio Code: [Latest version of Visual Studio Code](https://code.visualstudio.com/download). To work with .Net Core, see [how to configure Visual Studio Code to work with .Net Core](https://code.visualstudio.com/docs/languages/dotnet).
* Mac:
 * Visual Studio 2019 For Mac](https://visualstudio.microsoft.com/en/vs/mac/), with the .Net Core workload: [Configure Visual Studio 2019 for Mac with .Net Core](https://visualstudio.microsoft.com/vs/mac/net/).
 * Visual studio Code: [Latest version of Visual Studio Code](https://code.visualstudio.com/download). To work with .Net Core, see [how to configure Visual Studio Code to work with .Net Core](https://code.visualstudio.com/docs/languages/dotnet).
* Linux:
 * Visual studio Code: [The latest version of Visual Studio Code](https://code.visualstudio.com/download). For working with .Net Core, see [how to configure Visual Studio Code for working with .Net Core](https://code.visualstudio.com/docs/languages/dotnet).

#### Electron and Electron.Net

Once the development environment is installed and configured, you will need to install everything needed to run electron on your machine along with .Net Core.

* Install the NPM package manager, contained in [NodeJS](https://nodejs.org/en/)
* Install the [CLI of Electron.Net](https://www.nuget.org/packages/ElectronNET.CLI/). Check the installation instructions on the nuget page.

### Downloading the repository

1. Go to the repository at : [https://github.com/inclusive-thinking/photo-labeler](https://github.com/inclusive-thinking/photo-labeler).
2. Log in to Github with your user, if you haven't already.
3. Click on the "Fork" button to create a copy of the repository under your own user.
4. Once the fork is done, clone the repository you just created:
``bash
git clone https://github.com/your_user/photo-labeler
```
5. Enter the repository
 ``bash
cd photo-labeler
 ```
6. photo-labeler depends on a third party library called metadata-extractor-dotnet. This library is added as a sub-module, so we will have to initialize it:
``bash
git submodule update --init --recursive
```
7. Access the src/PhotoLabeler directory, and in the terminal, execute the command:
``bash
electronize start
```
8. After a few seconds, the application window should open and you can start using it. If you want to debug the application using Visual Studio or Visual Studio Code, join the "PhotoLabeler" process that should appear in the process list. Here is an article about debugging .Net Core applications with Visual Studio Code (https://medium.com/@mikezrimsek/debugging-dotnet-core-projects-with-visual-studio-code-ff0ab66ecc70).

### How to contribute?

The easiest way is to go to the [project issues] page (https://github.com/inclusive-thinking/photo-labeler/issues). There, you can see the open issues and contribute to any of them that nobody is working on yet. You can also create your own issue if you find a bug or propose a new feature. In the latter two cases, it would be wise to wait for a member of the project to confirm that the bug exists, or that the feature you want to add is consistent with the project.
1. Go to the issue and look at the number assigned to it (it's in the header, in the title and at the end of the URL).
2. Synchronize the develop branch of your repository with the develop branch of the parent repository (the one in inclusive-thinking/photo labeler). (Here's a guide on how to do this) (https://docs.github.com/en/github/collaborating-with-issues-and-pull-requests/syncing-a-fork).
3. Create a branch based on the newly synchronized develop branch called feature/[issue_number]-goal-very-short-scripted. For example: "feature/47-add-focus-indicator".
 ``bash
git checkout -b feature/47-add-focus-indicator
 ```
 If it's a bug, instead of a feature, prefix the name of the branch with "bugfix": bugfix/48-fix-typo-in-main-menu.
4. Add a comment to the issue, indicating that you are working on it. If you have already uploaded your new branch to your remote repository, it would be interesting to add the link to that branch, in case someone wants to check the progress of the work.
5. Work on your branch. If your work takes a few days, it would be good to resynchronize the develop branch with the latest changes in the parent repository, and merge the updated develop branch into your branch. The more up-to-date the branch you are working on is with the develop branch from the parent repository, the fewer conflicts there will be when you create the pull request to integrate your changes.
6. In the navigation section of your repository, click on "Pull requests" and then "Create pull request".
7. Compare your branch to the develop branch of the parent repository. If all went well, you should be informed that the branches can be merged.
8. Click on the "Create pull requests" button, and fill in the fields as requested.
9. In the description, link your PR to the issue you were working on. [Here is a guide to linking issues with pull requests](https://docs.github.com/en/enterprise/2.17/user/github/managing-your-work-on-github/linking-a-pull-request-to-an-issue). For example: Fixes inclusive-thinking/photo-labeler#47
10. Once you have finished filling in the fields, complete the creation of the pull request. You will be notified by mail of the changes, including the reviewers' comments on it.
11. Once your PR is approved and completed, your changes will be available in the develop branch and the linked issue will be closed automatically. Good work!

## How it works

The operation is very simple. The application consists of two buttons: "Select folder" and "Exit".

It also has two menus: file (called PhotoLabeler on Mac), and Language. In the language menu you can choose in which language you want to see the application, currently only English and Spanish are supported.

### Select folder

If you press the "Select folder" button, a system dialog will open where you can choose the directory you want to start working on.

**Important note: If you are using a MAC, during user tests it has been detected that when you click the button, although the system dialog opens, VoiceOver is not aware of this opening, and you will have to restart it without changing windows to start interacting with the dialog. There is already an open issue in Electron's repository talking about this topic: [Electron Issue: Dialog box message not read out by screen reader](https://github.com/electron/electron/issues/14234). I will be looking forward to this issue to update the documentation as soon as it is resolved.

Once you have chosen the directory, you will see two controls: a tree presentation with the subdirectories of the open directory, and then a table with the photos of the selected directory. In that table, you will see a column called "Label", which will show the label of the selected photo.
When you tabulate, you'll see a checkbox that lets you hide unlabeled photos, and after this checkbox, a button that lets you rename all of the labeled photos to match the file name.
### Rename photos
When you press this button, the system will ask you if you want to rename the tagged photos, or it will inform you that there are no photos to tag. In case there are photos to rename and we press the button "yes", the photos will be renamed, and the system will warn us when the operation is finished.

The photos will be renamed with a numerical prefix to maintain the order of the date of creation in ascending order. In future versions, this option will be configurable.

## Exit
 
This button allows you to exit the application.

## How to tag photos on the iPhone and rename them with PhotoLabeler
 
1. Open the "Photos.
2. Go to the album you want to tag.
3. Choose the photo you want to tag.
3.1. If you use Voice Over: Press twice with two fingers, keeping your fingers on the screen on the second press. Four beeps will sound, and on the fourth, a dialog will open to tag the item. This dialog is used to tag not only pictures, but also any element that we want to tag within any application. The beauty of tagging photos is that not only is the tag saved in VoiceOver, but when that photo is exported, the tag is attached to the photo's metadata. It is this metadata that PhotoLabeler uses to rename the file names (which are not at all descriptive) to match that label.
If you are using iOS 14, slide your finger up over the photo and you will see a field for adding a caption. Type in the label and click OK.
If you are not using VoiceOver and do not have iOS 14, you will not be able to tag your photos.
4. Once you have tagged a photo, go to the photo, and click the "Share" button. Choose the application you want to export the photo to. On MAC, the easiest way is to export with Airdrop, and on Windows or LInux, you can use storage applications such as Google Drive or Mega. Dropbox can also be used, but as far as I know, if we select several photos at once, the option does not appear in the share menu.
5. Once you have the photos on your computer, use the "Open folder" option of PhotoLabeler to access the directory where the photos are, and then press the "Rename photos" button. From then on, all the photos in the directory will have descriptive names, based on the label you previously put on your iPhone.

