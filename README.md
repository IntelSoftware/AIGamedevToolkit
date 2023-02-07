# AIGamedevToolkit
Foundation layer for AI Gamedev Toolkit which can be built upon by dev community

Welcome to the AI Game Development Toolkit! You can find more information about the toolkit here: https://www.intel.com/content/www/us/en/developer/articles/training/ai-gamedev-toolkit-tutorials.html

For more information about Gaia ML and the integration of the AI Gamedev Toolkit into Gaia, please visit this page: https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/advanced/ai-game-development-toolkit-r123/



## Demo Projects

* [Starter Demos](https://github.com/IntelSoftware/aigamedevtoolkit-starter-demos)

## Important Notes

**DLL Conflicts:** It is possible that other Unity assets depend on some of the same 3rd-party `.dll` files (e.g. `tbb.dll`) as the AIGamedevToolkit. Unity only allows one copy of a given `.dll` file in a project and will throw an error when there are duplicates. This issue can be resolved by removing the duplicate `.dll` files located in the `AIGamedevToolkit/InferenceEngines/OpenVINO/Plugins/x86_64/` subfolder.


## Change Log

2023-2-07: Upgrade OpenVINO to 2022.2 version
