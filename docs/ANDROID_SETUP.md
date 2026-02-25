# Android .NET SDK Setup Guide

DroidIDE requires a locally installed .NET SDK to build and run your projects. Since Android is a restricted environment, the best way to get .NET is via **Termux**.

Follow these steps carefully to set up your environment.

---

## Step 1: Install Termux

> [!IMPORTANT]  
> **Do NOT install Termux from the Google Play Store.** The Play Store version is outdated and will not work correctly.

1.  Download the latest **Termux APK** from one of these trusted sources:
    -   **F-Droid**: [Download Termux on F-Droid](https://f-droid.org/en/packages/com.termux/)
    -   **GitHub**: [Termux Releases](https://github.com/termux/termux-app/releases)
2.  Install the APK on your Android device.

---

## Step 2: Initialize the Environment

1.  Open the **Termux** app.
2.  Run the following commands one by one to ensure your package lists are up to date:
    ```bash
    pkg update
    pkg upgrade
    ```
    *If prompted with a (Y/n) question, type `y` and press Enter.*

---

## Step 3: Install .NET (Most Reliable Method)

Native Termux installation sometimes fails due to repository issues. The **proot-distro (Ubuntu)** method is the "gold standard" because .NET is officially supported on Ubuntu.

1.  **Install proot-distro**:
    ```bash
    pkg install proot-distro
    ```
2.  **Install Ubuntu**:
    ```bash
    proot-distro install ubuntu
    ```
3.  **Login to Ubuntu**:
    ```bash
    proot-distro login ubuntu
    ```
4.  **Install .NET Inside Ubuntu**:
    Once logged into Ubuntu (your prompt will change), run these commands:
    ```bash
    apt update && apt upgrade
    apt install dotnet-sdk-8.0
    ```
5.  **Important**: Keep Termux open in the background. DroidIDE will detect the SDK inside this container.

---

## Alternative: Native Termux Install (via TUR)

If you prefer to stay in the native Termux environment, try searching for the exact package name:

1.  **Enable TUR**: `pkg install tur-repo`
2.  **Search for the package**: `pkg search dotnet`
3.  **Install the output**: If it shows something like `dotnet-sdk-9.0`, run `pkg install dotnet-sdk-9.0`.

---

## Step 4: Verify the Installation

1.  Run this command to check if .NET is working:
    ```bash
    dotnet --version
    ```
2.  If you see a version number (like `8.0.x` or `9.0.x`), you are ready!

---

## Step 5: Connect DroidIDE

1.  Close and **Restart DroidIDE**.
2.  DroidIDE will automatically scan common Termux folders (like `/data/data/com.termux/files/usr/bin/dotnet`) to find the CLI.
3.  Open your project and tap **Build**.

---

## Troubleshooting

### "Permission Denied"
If you get permission errors in DroidIDE, ensure you have given DroidIDE "Files and Media" permissions in Android Settings.

### SDK not found
If DroidIDE still says "SDK not found", you can manually set the path in DroidIDE Settings (coming soon) or ensure your Termux is using the standard `/usr/bin/dotnet` location.

### Missing basic types (Red Squiggles)
Ensure you have upgraded to the latest version of DroidIDE (Fix 17+), which includes a robust metadata loader for Android.

---

## Ultimate Fix: The "Internal Storage Bridge" Protocol

> [!WARNING]  
> **The Sandbox Wall**: Even though your screenshot shows `8.0.124` is installed at `/usr/lib/dotnet`, that is a **virtual path** inside Ubuntu. To DroidIDE, that file is actually buried deep in Termux's private data folder.  
> **Android will NOT let DroidIDE open that file** because it belongs to Termux. This is why you get "No such file or directory."

Follow these steps to move the SDK into DroidIDE's private storage, where DroidIDE has full permission to run it:

1.  **Prepare the SDK in Termux (Ubuntu)**:
    ```bash
    # Zip the installed SDK
    tar -czvf dotnet-sdk.tar.gz /usr/lib/dotnet
    
    # Move it to your phone's public Downloads folder
    cp dotnet-sdk.tar.gz /sdcard/Download/
    ```
2.  **Move it to DroidIDE**:
    -   Open your phone's **File Manager**.
    -   Go to `Internal Storage > Download`.
    -   Find `dotnet-sdk.tar.gz`.
    -   Move/Copy it to: `Android > data > com.companyname.droidide.app > files`.
3.  **Extract (Recommended)**:
    -   If possible, use a file manager (like **ZArchiver**) to extract the folder there.
    -   The final path should look like: `/data/user/0/com.companyname.droidide.app/files/dotnet`.
4.  **Restart DroidIDE**:
    -   DroidIDE will now find the SDK in its own "Home" folder and will have full permission to run it.

---

### Memory Error: "GC heap initialization failed" (0x8007000E)
This is a common issue when running .NET inside a proot-distro container (like Ubuntu) on Android. It happens because .NET tries to reserve more virtual memory than allowed.

**Fix**: Run this command in your Ubuntu terminal before running `dotnet`:
```bash
export DOTNET_GCHeapHardLimit=1C000000
```
This limits the .NET memory usage to 448MB, which is safe for most Android devices. DroidIDE now sets this automatically for you when building and running!
