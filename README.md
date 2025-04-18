# Fairbanc - Data Submission App

The **Fairbanc - Data Submission App** is a Windows-based application designed to automate and manage data submission processes. Built using modern technologies, the app simplifies report submissions by offering features like automatic scheduling, manual uploads, and comprehensive logging.

<a href="https://dotnet.microsoft.com/id-id/languages/csharp">
  <img src="https://sharpminds.com/wp-content/uploads/2022/06/c-sharp.svg" width="150" heigh="150" alt="CSharpp">
</a>
<a href="https://learn.microsoft.com/en-us/windows/apps/winui/">
   <img src="https://betanews.com/wp-content/uploads/2021/02/winui.jpg" height="180" alt="WinUI_33">                                                             
</a>

<a href="https://learn.microsoft.com/en-us/windows/apps/develop/data-binding/data-binding-and-mvvm">
   <img src="https://www.remoterocketship.com/_next/image?url=%2Fimages%2Fblog%2FMVVM-icon-for-blog.jpg&w=640&q=75" height="180" alt="Mvvm"> 
</a>                                                                 
<br></br>

<a href="https://apps.microsoft.com/detail/9nlrw8q7pq2c?cid=DevShareMCLPCS&hl=en-US&gl=ID">
  <img src="https://www.safetysign.com/images/source/large-images/F2010.png" width="100" alt="Build Status">
</a>




## Features

### 1. Automatic Data Submission
- Schedule monthly report submissions with configurable dates and times.
- Automatically notify the Fairbanc team via Slack upon successful submissions.
- Supports configuration of Excel file types with keywords for Sales, Payments, and Customer data.

### 2. Manual Data Submission 
- Drag-and-drop interface for uploading sales, payments, and customer data files.
- Select specific periods (month and year) for manual submissions.
- Process multiple file types with a streamlined interface.

### 3. Submission History
- View a detailed log of all previous submissions.
- Track submission statuses for better transparency and accountability.

### 4. Configuration Options
- Configure report submission settings once during the first login.
- Specify file locations, keywords, and submission times.
- Save and reuse configurations for future submissions.

### 5. Slack Integration
- Notifications are sent to the `3report-ind-test` Slack channel.
- Includes zipped files and log details for every submission.

## Tech Stack

The application leverages the following technologies:

- **WinUI 3**: Modern UI framework for Windows applications.
- **C#**: Primary programming language for application logic and development.
- **.NET Generic Host**: Used for dependency injection, configuration, and logging.
- **Serilog**: Logging framework for application diagnostics.
- **Community Toolkit for MVVM**: Implements the MVVM design pattern for clean and maintainable code.
- **Slack Integration**: Enables notifications for submission updates.

## How to download the app ###

### Published app (in Indonesian Windows Store) ###
Note: Please change your computer Windows settings -> Region to Indonesia - to be able to view it.
1. Download the App from Microsoft Store, here:   
   [![Donwload](https://get.microsoft.com/images/en-us%20light.svg)](https://apps.microsoft.com/detail/9nlrw8q7pq2c?cid=DevShareMCLPCS&hl=en-US&gl=ID)
