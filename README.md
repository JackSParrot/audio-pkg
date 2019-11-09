# es.jacksparrot.audio unity package

## Installation using the Unity Package Manager (Unity 2018.1+)

The [Unity Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html) (UPM) is a new method to manage external packages. It keeps package contents separate from your main project files.

1. Modify your project's `Packages/manifest.json` file adding this line:

    ```json
    "es.jacksparrot.audio": "https://github.com/JackSParrot/audio-pkg.git"
    ```

    Make sure it's still a valid JSON file. For example:

    ```json
    {
        "dependencies": {
            "com.unity.package-manager-ui": "2.2.0",
            "es.jacksparrot.audio": "https://github.com/JackSParrot/audio-pkg.git"
        }
    }
    ```

2. To update the package you need to delete the package lock entry in the `lock` section in `Packages/manifest.json`. The entry to delete could look like this:

    ```json
    "es.jacksparrot.audio": {
      "hash": "a7ffd9287ac3c0ce1c68204873d24e540b88940d",
      "revision": "HEAD"
    }
    ```
