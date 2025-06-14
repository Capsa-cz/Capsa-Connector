# Capsa Connector

Capsa Connector is a program designed to facilitate communication with a server, manage files, and handle updates. It provides the following key functionalities:

## Features

- **Server Communication**: Handles communication with the server for file operations and updates.
- **File Management**: Opens files, monitors changes, and uploads them back to the server.
- **Auto-Updating**: Automatically checks for and installs new versions from the server.
- **Network Drive Integration**: Connects local network drives using SSHFS and WinFSP for seamless file access.

## Technologies Used

- **C#**: Core programming language for the application.
- **RestSharp**: For handling REST API communication.
- **NetSparkleUpdater**: For managing application updates.
- **SSHFS and WinFSP**: For mounting remote file systems as local drives.

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/capsa-connector-win.git
   ```
2. Install dependencies:
   - Ensure that [WinFSP](https://winfsp.dev/) and [SSHFS](https://github.com/winfsp/sshfs) are installed on your system.
3. Build the project using Visual Studio or your preferred IDE.

## Configuration

- The application uses a `.env` file for sensitive configurations. Create a `.env` file in the root directory based on the provided `.env.example` file:
  ```plaintext
  X_CAPSA_ORIGIN=windows-app
  X_CAPSA_ORIGIN_SECRET=your-secret-key
  ```

## Usage

- Run the application to start managing files and connecting to the server.
- The application will automatically check for updates and notify you when a new version is available.
- You can build custom API on top of the provided App to extend its functionality.

## License

This project is licensed under the Mozilla Public License 2.0.  
See the full license text in the [LICENSE](./LICENSE) file.
