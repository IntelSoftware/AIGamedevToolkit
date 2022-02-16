# Getting Started HDRP



* [Overview](#overview)
* [Clone Toolkit Repository](#clone-toolkit-repository)
* [Create Unity Project](#create-unity-project)
* [Add Toolkit Folder](#add-toolkit-folder)
* [Add Inference Features](#add-inference-features)
* [Allow Unsafe Code](#allow-unsafe-code)
* [Install Barracuda Package](#install-barracuda-package)
* [Render Pipeline Setup](#render-pipeline-setup)
* [Add Volume Component](#add-volume-component)
* [Test it Out](#test-it-out)



## Overview

In this tutorial we will provide a basic demonstration of how to incorporate the [AIGamedevToolkit](https://www.intel.com/content/www/us/en/developer/articles/training/ai-gamedev-toolkit-tutorials.html) into a [Unity](https://unity.com/) project which uses the [High Definition Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@10.8/manual/index.html) (HDRP). By the end, you will know how to run a [deep learning](https://community.intel.com/t5/Blogs/Tech-Innovation/Artificial-Intelligence-AI/The-Difference-Between-Artificial-Intelligence-Machine-Learning/post/1335666) model inside a Unity HDRP scene, without any additional coding.

> **Note:** Please read this [introductory tutorial](https://github.com/IntelSoftware/AIGamedevToolkit/blob/main/Documentation~/Getting%20Started.md) before continuing.



## Clone Toolkit Repository

First, we need to clone the [GitHub repository](https://github.com/IntelSoftware/AIGamedevToolkit) for the toolkit. Make sure to actually clone the repository rather than downloading it as a `.zip`. Compressing the project folder can break some of the binary files included in the toolkit.



## Create Unity Project

Next, we need to create a HDRP project to use the toolkit. We can stick with the HDRP sample scene provided by Unity.

![create_new_unity_hdrp_project](images/getting-started-hdrp/create-new-unity-hdrp-project.png)



## Add Toolkit Folder

Once the Unity Editor has loaded, we can add the toolkit folder. Open the AIGamedevToolkit repository folder and select the `AIGamedevToolkit` subfolder.

![select-toolkit-folder](images/getting-started-hdrp/select-toolkit-folder-hdrp.png)

Drag the toolkit folder into the `Project → Assets` directory.

![add_toolkit_folder](images/getting-started-hdrp/add-toolkit-folder-hdrp.png)



## Add Inference Features

As mentioned in the previous tutorial, the toolkit provides a graphical user interface (GUI) for adding inference features to a scene.

![toolkit_menu](images/getting-started-hdrp/toolkit-menu-hdrp.png)



## Allow Unsafe Code

Once again, unsafe code needs to be enabled to use OpenVINO inference features.

![toolkit-inference-feature-window](images/getting-started-hdrp/toolkit-inference-feature-window.png)

Unsafe code can be enabled in the the Player Settings, just like when using the Built-in Render Pipeline.

![scroll-to-unsafe-code-option](images/getting-started-hdrp/scroll-to-unsafe-code-option-hdrp.png)



## Install Barracuda Package

Next, we will install the Barracuda package to enable the Barracuda inference features.

![add-barracuda-from-git](images/getting-started-hdrp/add-barracuda-from-git-hdrp.png)



Back in the AIGamedevToolkit window, deselect the `COCO_YOLOX` inference feature and select the `StyleTransfer_Barracuda` option. Click on `Apply now` to add the inference feature to the scene.

![apply-inference-feature-to-scene](images/getting-started-hdrp/apply-inference-feature-to-scene-hdrp.png)



## Render Pipeline Setup

We need to let the toolkit know which Render Pipeline we are using. Open the `AI Gamedev Toolkit` submenu and select `Render Pipeline Setup...`.

![open-render-pipeline-setup](images/getting-started-hdrp/open-render-pipeline-setup.png)

The toolkit will detect the current render pipeline and offer to perform the required configuration steps. Click `Yes` in the popup window to set up the toolkit for HDRP.

![perform-render-pipeline-setup](images/getting-started-hdrp/perform-render-pipeline-setup.png)



## Add Volume Component 

We can access the texture data for the current frame with a [custom volume component](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@10.8/api/UnityEngine.Rendering.HighDefinition.CustomPostProcessVolumeComponent.html). The toolkit includes a preconfigured volume component called `HDRPTextureHelper`.

![hdrptexturehelper-file-location](images/getting-started-hdrp/hdrptexturehelper-file-location.png)

It provides the same functionality as the `CameraTextureHelper.cs` script used for the Built-in Render Pipeline. We need to add this volume component to the `Default Volume Profile Asset` in the Project Settings. Open the Project Settings folder and select the `HDRP Default Settings` submenu.

![hdrp-default-settings-2020](images/getting-started-hdrp/hdrp-default-settings-2020.png)

> **Note:** In Unity 2021, the HDRP settings are located at `Project Settings > Graphics > HDRP Default Settings`
>
> ![hdrp-default-settings-2021](images/getting-started-hdrp/hdrp-default-settings-2021.png)



Click on the `Add Override` button under the `Default Volume Profile Asset`.

![add-volume-component-override](images/getting-started-hdrp/add-volume-component-override.png)

Type `HDRP` into the search box and select `HDRPTextureHelper`

![add-hdrptexturehelper-override](images/getting-started-hdrp/add-hdrptexturehelper-override.png)



As with the `CameraTextureHelper.cs` script, the `HDRPTextureHelper` has a list of `InputRenderTexture` assets. Click on the toggle next to the `Input Textures Param` field to make it editable.

![make-inputtextureparam-editable](images/getting-started-hdrp/make-inputtextureparam-editable.png)

Open the `Input Textures Param` dropdown and click on the `+` sign.

![add-new-entry-to-inputtexturesparam](images/getting-started-hdrp/add-new-entry-to-inputtexturesparam.png)

We can use the same `MainCamer_Texture` asset we used in the previous tutorial.

![add-maincamera-texture-asset](images/getting-started-hdrp/add-maincamera-texture-asset.png)



Scroll down to the `After Post Process` field and click on the `+` sign to add a new entry.

![add-to-after-post-process](images/getting-started-hdrp/add-to-after-post-process.png)

Select `AIGamedevToolkit.HDRPTextureHelper` from the available options.

![add-texture-helper-to-after-post-process](images/getting-started-hdrp/add-texture-helper-to-after-post-process.png)





## Test it Out

Now we can press play to see the style transfer inference feature applied to the scene.

![test-hdrp-barracuda-style-transfer](images/getting-started-hdrp/test-hdrp-barracuda-style-transfer.png)

Additionally, the YOLOX inference feature can detect in-game objects.

![test-hdrp-yolox-1](images/getting-started-hdrp/test-hdrp-yolox-1.png)



![test-hdrp-yolox-2](images/getting-started-hdrp/test-hdrp-yolox-2.png)





