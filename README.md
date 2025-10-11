<a id="readme-top"></a>

<!-- ABOUT THE PROJECT -->
## About The Project

The 1st lab project for the OOP course, aimed at an advanced level.

This project includes all functions from the prerequisite, as well as providing basic spreadsheet functionality, including:
- Editing cells with text or mathematical expressions
- Detecting and preventing cyclic dependencies between cells
- Automatic recalculation of dependent cells
- Saving and loading spreadsheets from local storage
- Synchronizing files with Google Drive
- Ukrainian interface with functionality to expand it to other languages as well

This project is oriented for using on Windows devices only and most likely is not secure by any means.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



### Built With

* .NET 9.0
* MAUI 9.0.82
* MAUI Community Toolkit 12.2.0
* Google APIs Auth 1.72.0
* Google APIs Drive v3 1.71.0.3905
* Google APIs OAuth2 v2 1.68.0.1869

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

This section will guide you through installing requirements for this project. I have not tested it outside of my system and outside of the program versions I have specified, so I do not know if it will work on earlier versions.

### Prerequisites

1. Install .NET 9.0: https://dotnet.microsoft.com/en-us/download/dotnet/9.0
2. Install MAUI by executing the following command: `dotnet workload install maui`

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/amiadesu/OOP-Lab1-ExcelClone.git
   ```
2. Install requirements
   ```sh
   dotnet restore
   ```
3. Get a UWP Google Drive API client ID by following these guides: https://developers.google.com/workspace/drive/api/guides/about-sdk, https://developers.google.com/identity/gsi/web/guides/get-google-api-clientid
4. Make your own `Secrets.cs` at `./Source/Constants` by following the guide inside `Secrets.cs.example` that is located in the same directory.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

Once you installed all requirements, you should execute a `dotnet build` command to build both project and tests.

To run the project you should go inside `./Source/bin/` folder and follow the folders that correspond to the type of project you've built and the .NET version with which you built the project.

To test the project you should execute a `dotnet test` command.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://github.com/amiadesu/OOP-Lab1-ExcelClone/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=amiadesu/VocaDBScraper" alt="contrib.rocks image" />
</a>

<!-- LICENSE -->
## License

Distributed under the MIT. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
