# Fairbanc - Data Submission App

The **Fairbanc - Data Submission App** is a Windows-based application designed to automate and manage data submission processes. Built using modern technologies, the app simplifies report submissions by offering features like automatic scheduling, manual uploads, and comprehensive logging.

## Features

### 1. Automatic Data Submission
- Schedule monthly report submissions with configurable dates and times.
- Automatically notify the Fairbanc team via Slack upon successful submissions.
- Supports configuration of Excel file types with keywords for Sales, Payments, and Customer data.

### 2. Manual Data Submission *(In Development)*
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

## Getting Started

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/TheFairbanc/FTIDataSharingUIonWinUI3.git
