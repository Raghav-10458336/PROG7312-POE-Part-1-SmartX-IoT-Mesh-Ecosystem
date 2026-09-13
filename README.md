# PROG7312 POE Part 1 Smart-X IoT Mesh Ecosystem
PROG7312 POE Part 1: Smart-X IoT Mesh Ecosystem data ingestion, validation and telemetry management application built using .NET 10.

## PROG7312 POE Part 1 — Data Ingestion and Validation Gateway

**Student:** Raghav Mahraj  
**Student Number:** ST10458336  

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Technology Stack](#technology-stack)
4. [Project Structure](#project-structure)
5. [Core Features](#core-features)
6. [Sensor Registration](#sensor-registration)
7. [Telemetry Ingestion](#telemetry-ingestion)
8. [Mock IoT Deployment](#mock-iot-deployment)
9. [Generics](#generics)
10. [Jagged Arrays and Collections](#jagged-arrays-and-collections)
11. [Operator Overloading](#operator-overloading)
12. [Recursive Deployment Validation](#recursive-deployment-validation)
13. [Attention Centre](#attention-centre)
14. [Sensor Connectivity Monitoring](#sensor-connectivity-monitoring)
15. [Secure File Attachments](#secure-file-attachments)
16. [API Endpoints](#api-endpoints)
17. [Application Setup](#application-setup)
18. [Running the Application](#running-the-application)
19. [Testing the Application](#testing-the-application)
20. [Important Development Notes](#important-development-notes)

---

# Project Overview

Smart-X is an IoT management gateway developed for the **PROG7312 POE Part 1**.

The system is designed to model a distributed IoT environment in which devices such as ESP32 microcontrollers continuously transmit different forms of telemetry to a central gateway.

Example Smart-X deployment environments include:

- Hydroponic farming systems
- Automated utility monitoring
- Smart building infrastructure
- Power-consumption monitoring
- Distributed environmental sensing
- Automated actuator systems

IoT devices can transmit different data types depending on their purpose. Smart-X therefore supports environmental, power-consumption and actuator telemetry through a strongly typed .NET architecture.

The Part 1 implementation focuses on the first Smart-X management pillar:

> **Sensor Data Ingestion and Telemetry**

The remaining Smart-X pillars are displayed in the startup interface but are intentionally unavailable during Part 1:

- **Real-Time Command Stream and History** — Part 2
- **Network Topology and Mesh Routing** — Final POE

---

# System Architecture

Smart-X follows a web-based client/API architecture consisting of three .NET 10 projects.

```text
┌─────────────────────────────────────────────┐
│            SmartX.Client                    │
│         Blazor WebAssembly                  │
│                                             │
│  Dashboard • Sensors • Telemetry • Alerts   │
│  Attachments • Deployment Validation        │
└─────────────────────┬───────────────────────┘
                      │
                      │ HTTPS / REST API
                      │ JSON
                      ▼
┌─────────────────────────────────────────────┐
│              SmartX.Api                     │
│         ASP.NET Core Web API                │
│                                             │
│  Telemetry Ingestion                        │
│  Sensor Registry                            │
│  Alert Processing                           │
│  Attachment Storage                         │
│  Deployment Validation                      │
│  Power Analysis                             │
└─────────────────────┬───────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────┐
│             SmartX.Shared                   │
│        Shared .NET Class Library            │
│                                             │
│  Models • Generics • Collections            │
│  Validation • Enums                         │
└─────────────────────────────────────────────┘
```

## SmartX.Api

The ASP.NET Core Web API acts as the central Smart-X gateway.

It is responsible for:

- Receiving telemetry packets
- Processing typed telemetry
- Maintaining the sensor registry
- Providing telemetry data to the frontend
- Detecting anomalous sensor readings
- Detecting sensor connectivity problems
- Managing alert acknowledgement
- Processing power-reading comparisons
- Validating nested deployment structures
- Receiving and protecting sensor attachments
- Generating the simulated IoT workload

## SmartX.Client

The Blazor WebAssembly application provides the Smart-X management interface.

It communicates asynchronously with the Web API and provides interfaces for:

- Gateway monitoring
- Sensor registration and management
- Telemetry submission
- Telemetry inspection and filtering
- Power-load analysis
- Alert investigation
- Alert acknowledgement
- Sensor attachment uploads
- Deployment validation

## SmartX.Shared

The shared class library contains reusable models and application logic required across the solution.

This prevents common structures from being unnecessarily duplicated between the API and frontend.

---

# Technology Stack

| Technology | Purpose |
|---|---|
| .NET 10 | Application framework |
| C# | Primary programming language |
| ASP.NET Core Web API | Backend IoT gateway |
| Blazor WebAssembly | Web management interface |
| HTML / CSS | Interface structure and styling |
| REST | Client/API communication |
| JSON | API data exchange |
| HTTPS | Secure API communication |
| AES | Protection of stored attachments |
| Git | Version control |
| GitHub | Source repository |
| Postman | API testing |
| Visual Studio 2026 | Development environment |

---

# Project Structure

The solution contains three projects:

```text
PROG7312.POE.Part1
│
├── SmartX.Api
│   │
│   ├── Controllers
│   │   ├── AlertsController.cs
│   │   ├── AttachmentsController.cs
│   │   ├── DeploymentController.cs
│   │   ├── PowerAnalysisController.cs
│   │   ├── SensorsController.cs
│   │   └── TelemetryController.cs
│   │
│   ├── Services
│   │   ├── AttachmentStorageService.cs
│   │   ├── MockTelemetrySeeder.cs
│   │   ├── SensorRegistryService.cs
│   │   ├── TelemetryAlertService.cs
│   │   └── TelemetryIngestionService.cs
│   │
│   ├── EncryptedUploads
│   ├── Program.cs
│   └── appsettings.json
│
├── SmartX.Client
│   │
│   ├── Pages
│   │   ├── Alerts.razor
│   │   ├── Attachments.razor
│   │   ├── Dashboard.razor
│   │   ├── Deployment.razor
│   │   ├── Home.razor
│   │   ├── Sensors.razor
│   │   └── Telemetry.razor
│   │
│   ├── Layout
│   ├── wwwroot
│   ├── App.razor
│   └── Program.cs
│
└── SmartX.Shared
    │
    ├── Enums
    │   └── SensorCategory.cs
    │
    ├── Models
    │   ├── DeploymentNode.cs
    │   ├── PowerAnalysisResult.cs
    │   ├── PowerReading.cs
    │   ├── SensorAttachment.cs
    │   ├── SensorRegistration.cs
    │   └── TelemetryAlert.cs
    │
    ├── Telemetry
    │   ├── TelemetryBatch.cs
    │   ├── TelemetryBuffer.cs
    │   └── TelemetryPacket.cs
    │
    └── Validation
        └── DeploymentValidator.cs
```

---

# Core Features

Smart-X Part 1 provides the following functionality:

- Professional Smart-X startup gateway
- Sensor registration and management
- Environmental telemetry ingestion
- Power telemetry ingestion
- Actuator telemetry ingestion
- Strongly typed generic telemetry packets
- High-volume mock IoT telemetry generation
- Jagged-array telemetry batching
- Generic telemetry collections
- Filterable telemetry monitoring
- Bounded telemetry retrieval
- Power-load analysis using operator overloading
- Automatic anomaly detection
- Sensor connectivity monitoring
- Alert acknowledgement
- Secure sensor attachment upload
- Encrypted attachment storage
- Recursive deployment hierarchy validation
- Responsive management dashboard

---

# Sensor Registration

Smart-X maintains a registry of IoT devices known to the gateway.

Each sensor registration contains:

- Unique device identifier
- Deployment location
- Sensor category
- Registration timestamp
- Active status

Three sensor categories are supported:

```text
Environmental
Power Consumption
Actuator
```

Example registered devices include:

```text
ESP32-ENV-01
ESP32-PWR-01
ESP32-ACT-01
```

Each device is also associated with a deployment location, for example:

```text
Hydroponic Zone 1
Utility Node 1
Actuator Node 1
```

The Sensor Registry interface supports:

- Creating sensor registrations
- Viewing registered sensors
- Selecting a sensor category
- Duplicate device identifier protection
- Input validation
- Removing sensors
- Delete confirmation before removal

---

# Telemetry Ingestion

Smart-X supports three telemetry data types.

| Sensor Type | C# Type | Example Metrics |
|---|---|---|
| Environmental | `float` | Soil Moisture, Temperature, Humidity |
| Power | `int` | Power Draw, Energy Usage, Flow Rate |
| Actuator | `bool` | Valve State, Pump State, Relay State |

The frontend provides a telemetry ingestion interface that allows a registered sensor to be selected and a reading submitted to the gateway.

The correct telemetry endpoint is selected according to the telemetry type.

```text
POST /api/telemetry/float
POST /api/telemetry/integer
POST /api/telemetry/boolean
```

The interface validates telemetry before submission.

For example:

- A sensor must be selected.
- A telemetry metric must be selected.
- A unit must be provided.
- Float telemetry accepts decimal values.
- Integer telemetry requires whole-number values.
- Boolean telemetry accepts true/false states.

After successful ingestion, the reading becomes part of the gateway workload and can be retrieved through the telemetry interface.

---

# Mock IoT Deployment

Smart-X simulates a distributed IoT deployment so that the data structures and gateway can be demonstrated under a substantial telemetry workload without requiring physical microcontrollers.

The simulated deployment contains:

| Sensor Category | Registered Devices |
|---|---:|
| Environmental | 8 |
| Power Consumption | 8 |
| Actuator | 8 |
| **Total** | **24** |

At startup, the gateway seeds:

| Telemetry Type | Packets |
|---|---:|
| Float | 2,400 |
| Integer | 2,000 |
| Boolean | 1,600 |
| **Total** | **6,000** |

The mock data contains realistic variation across metrics and also introduces controlled abnormal readings.

This allows telemetry ingestion, collection processing, anomaly detection and dashboard behaviour to be demonstrated against thousands of readings.

---

# Generics

Smart-X uses a reusable generic telemetry packet:

```csharp
TelemetryPacket<T>
```

The generic type allows one telemetry structure to safely represent multiple underlying data types.

Examples include:

```csharp
TelemetryPacket<float>
TelemetryPacket<int>
TelemetryPacket<bool>
```

A packet contains information such as:

```text
Packet ID
Sensor ID
Device Identifier
Metric Name
Unit
Value
Timestamp
```

Using `TelemetryPacket<T>` avoids creating separate packet models for every telemetry primitive while retaining strongly typed values.

---

# Jagged Arrays and Collections

Smart-X uses jagged arrays when generating sequential batches of raw telemetry.

Examples include:

```csharp
TelemetryPacket<float>[][]
TelemetryPacket<int>[][]
TelemetryPacket<bool>[][]
```

These arrays model multiple batches of readings arriving from distributed IoT devices.

The generic:

```csharp
TelemetryBatch<T>
```

class processes these raw batches and converts them into collections for ingestion.

Telemetry is subsequently maintained using the reusable:

```csharp
TelemetryBuffer<T>
```

structure.

This provides a clear processing path:

```text
IoT Readings
      ↓
Jagged Telemetry Batches
      ↓
TelemetryBatch<T>
      ↓
Generic Collections
      ↓
TelemetryBuffer<T>
      ↓
Gateway API
```

---

# Operator Overloading

The `PowerReading` model demonstrates C# operator overloading for power telemetry.

The following operators are implemented:

```text
+
-
>
<
```

The Smart-X Telemetry interface includes a Power Load Analysis section.

Two power readings can be submitted to the API:

```text
Meter A
Meter B
```

The backend creates `PowerReading` objects and processes them using the overloaded operators.

The result provides:

- Meter A reading
- Meter B reading
- Combined power load
- Difference between readings
- Highest-load meter

For example:

```text
Meter A:          1,400 W
Meter B:          1,890 W
Combined Load:    3,290 W
Load Difference:    490 W
Highest Load:     Meter B
```

This demonstrates operator overloading as part of the running Smart-X system.

---

# Recursive Deployment Validation

IoT installations can contain multiple nested deployment levels.

For example:

```text
Facility A
└── Hydroponic Zone
    └── Sub-Zone
        └── Sensor Node
```

Smart-X represents this hierarchy using:

```csharp
DeploymentNode
```

Each deployment node can contain a collection of child nodes.

The:

```csharp
DeploymentValidator.ValidateNode()
```

method recursively traverses the deployment hierarchy.

The validation process checks the current node and then recursively validates each child until the complete hierarchy has been inspected.

If any nested node is invalid or incorrectly configured, the complete deployment is considered invalid.

The Deployment Validation interface allows this behaviour to be tested through the frontend and API.

---

# Attention Centre

Smart-X includes a dedicated **Attention Centre** for abnormal IoT conditions.

Instead of requiring an operator to manually inspect thousands of telemetry packets, incoming telemetry is evaluated against defined operational thresholds.

Examples of monitored conditions include:

### Environmental

- Low soil moisture
- Excessive temperature
- Low humidity

### Power and Utility

- Excessive power draw
- Excessive energy usage
- Abnormal flow rate

### Connectivity

- Sensors that have stopped reporting telemetry

Alerts are classified by severity:

```text
Critical
Warning
```

The Attention Centre displays:

- Total alerts
- Alerts requiring attention
- Critical alerts
- Warnings
- Device identifier
- Affected metric
- Alert description
- Detection time
- Acknowledgement state

---

# Alert Acknowledgement

Alerts requiring operator attention can be acknowledged from the Attention Centre.

When an alert is acknowledged:

- The alert remains available in the alert collection.
- Its acknowledgement status is updated.
- The acknowledgement timestamp is recorded.
- It is removed from the active attention-required count.

This provides a practical workflow for reviewing abnormal IoT conditions instead of simply displaying passive warning messages.

---

# Sensor Connectivity Monitoring

Smart-X also monitors simulated device connectivity.

The gateway evaluates when telemetry was last received from registered sensors.

If a device has not reported telemetry within the configured connectivity threshold, a critical connectivity alert can be generated.

An example connectivity alert is:

```text
ESP32-ENV-08 at Hydroponic Zone 8 has not reported telemetry
for approximately 35 minutes.
```

Connectivity alerts are identified using the:

```text
Sensor Connectivity
```

metric.

This makes communication failures clearly distinguishable from abnormal telemetry values.

---

# Telemetry Monitoring and Filtering

The Telemetry Stream provides a central interface for inspecting readings processed by the gateway.

Operators can filter telemetry by:

- Device
- Metric
- Telemetry type

Supported telemetry types include:

```text
Environmental
Power
Actuator
```

Boolean values are converted into meaningful operational states.

Examples include:

```text
Valve State: Open / Closed
Pump State: Running / Stopped
Relay State: Enabled / Disabled
```

The interface also displays the timestamp associated with each reading.

---

# Bounded Telemetry Retrieval

The gateway contains thousands of telemetry packets, but the frontend does not need to retrieve the complete workload every time the Telemetry page is opened.

The telemetry API therefore supports bounded retrieval using the `take` parameter.

Examples:

```text
GET /api/telemetry/float?take=250
GET /api/telemetry/integer?take=250
GET /api/telemetry/boolean?take=250
```

The API validates the requested amount and limits `take` to:

```text
1 - 1000
```

Requests outside the supported range are rejected with an HTTP `400 Bad Request`.

This prevents unnecessarily large telemetry responses and keeps the management interface responsive.

---

# Telemetry Summary

A lightweight summary endpoint provides gateway workload totals without requiring the client to retrieve all stored packets.

```text
GET /api/telemetry/summary
```

Example response:

```json
{
  "totalPackets": 6000,
  "floatPackets": 2400,
  "integerPackets": 2000,
  "booleanPackets": 1600
}
```

The original total-count endpoint is also available:

```text
GET /api/telemetry/count
```

---

# Secure File Attachments

Smart-X allows supporting files to be associated with registered sensors.

Examples include:

- Device configuration files
- Hardware logs
- Deployment photographs
- Diagnostic information
- PDF documentation
- Spreadsheet records

Supported file extensions include:

```text
.txt
.log
.json
.xml
.csv
.pdf
.docx
.xlsx
.jpg
.jpeg
.png
```

The maximum accepted file size is:

```text
10 MB
```

The attachment interface allows an operator to:

1. Select a registered sensor.
2. Select a supported file.
3. Upload the attachment through the API.
4. View files associated with the selected sensor.
5. Confirm that the file is stored in encrypted form.

---

# Attachment Security

Uploaded sensor records are protected through several controls.

## HTTPS Transport

The Blazor client communicates with the Smart-X API using HTTPS during development.

This protects data while it is transmitted between the client and gateway.

## Encrypted Storage

Uploaded files are encrypted before being stored by the gateway.

Stored files are placed in:

```text
SmartX.Api/EncryptedUploads/
```

The original file contents are therefore not stored as ordinary plaintext files.

## Generated Storage Names

Physical files are stored using generated identifiers rather than directly trusting the original user-supplied filename.

## File Validation

The API validates:

- Selected sensor
- File existence
- File size
- File extension

## Source Control Exclusion

The `EncryptedUploads` directory is excluded through `.gitignore`.

Uploaded files are therefore not committed to the Git repository.

---

# Encryption Key Configuration

The attachment encryption master key is intentionally **not stored in the repository**.

The application reads the key using the configuration path:

```text
AttachmentEncryption:MasterKey
```

For local development, the key is stored using **.NET User Secrets**.

The actual secret value must not be added to:

```text
README.md
appsettings.json
source code
Git commits
```

A local encryption key can be configured from the `SmartX.Api` project directory.

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Generate a random 256-bit key using PowerShell:

```powershell
[Convert]::ToBase64String(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
)
```

Then save the generated value:

```bash
dotnet user-secrets set "AttachmentEncryption:MasterKey" "YOUR-GENERATED-BASE64-KEY"
```

The private development key is deliberately excluded from this repository.

---

# API Endpoints

## Sensor API

### Retrieve all sensors

```http
GET /api/sensors
```

### Retrieve a sensor

```http
GET /api/sensors/{id}
```

### Register a sensor

```http
POST /api/sensors
```

### Remove a sensor

```http
DELETE /api/sensors/{id}
```

---

## Telemetry API

### Submit environmental telemetry

```http
POST /api/telemetry/float
```

### Submit power telemetry

```http
POST /api/telemetry/integer
```

### Submit actuator telemetry

```http
POST /api/telemetry/boolean
```

### Retrieve environmental telemetry

```http
GET /api/telemetry/float
```

Optional bounded request:

```http
GET /api/telemetry/float?take=250
```

### Retrieve power telemetry

```http
GET /api/telemetry/integer
```

Optional bounded request:

```http
GET /api/telemetry/integer?take=250
```

### Retrieve actuator telemetry

```http
GET /api/telemetry/boolean
```

Optional bounded request:

```http
GET /api/telemetry/boolean?take=250
```

### Retrieve total packet count

```http
GET /api/telemetry/count
```

### Retrieve telemetry summary

```http
GET /api/telemetry/summary
```

---

## Alert API

### Retrieve alerts

```http
GET /api/alerts
```

### Retrieve alert counts

```http
GET /api/alerts/count
```

The alert API also supports the acknowledgement operation used by the Attention Centre.

---

## Power Analysis API

```http
GET /api/power-analysis?meterA={value}&meterB={value}
```

Example:

```text
/api/power-analysis?meterA=1400&meterB=1890
```

---

## Deployment Validation API

```http
POST /api/deployment/validate
```

The request contains the root deployment node and its nested child hierarchy.

---

## Attachment API

### Upload attachment

```http
POST /api/sensors/{sensorId}/attachments
```

The file is submitted as multipart form data.

### Retrieve sensor attachments

```http
GET /api/sensors/{sensorId}/attachments
```

---

# Application Setup

## Requirements

Before running the solution, ensure the following are installed:

- Visual Studio 2026
- .NET 10 SDK
- Git
- Modern web browser

Postman can also be used for direct API testing.

---

## Step 1 — Clone the Repository

Clone the repository using Git:

```bash
git clone <repository-url>
```

Alternatively, clone the repository through Visual Studio or GitHub Desktop.

---

## Step 2 — Open the Solution

Navigate to the cloned repository and open:

```text
PROG7312.POE.Part1.sln
```

using Visual Studio 2026.

---

## Step 3 — Restore Dependencies

Visual Studio should restore required NuGet packages automatically.

Dependencies can also be restored manually from the solution directory:

```bash
dotnet restore
```

---

## Step 4 — Configure the Encryption Key

Navigate to:

```text
SmartX.Api
```

Initialize User Secrets if they have not already been initialized:

```bash
dotnet user-secrets init
```

Generate a 256-bit Base64 key in PowerShell:

```powershell
[Convert]::ToBase64String(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
)
```

Store the generated key:

```bash
dotnet user-secrets set "AttachmentEncryption:MasterKey" "YOUR-GENERATED-BASE64-KEY"
```

Do not place the real key in `appsettings.json`.

---

## Step 5 — Configure Startup Projects

Both the API and frontend must run for the complete application.

In Visual Studio:

1. Right-click the solution.
2. Select **Configure Startup Projects**.
3. Select **Multiple startup projects**.
4. Set `SmartX.Api` to **Start**.
5. Set `SmartX.Client` to **Start**.
6. Leave `SmartX.Shared` without a startup action.
7. Apply the configuration.

---

## Step 6 — Build the Solution

From Visual Studio:

```text
Build → Build Solution
```

Alternatively:

```bash
dotnet build
```

The three projects should compile successfully.

---

# Running the Application

Start the solution using Visual Studio.

The development configuration currently uses:

### Smart-X API

```text
https://localhost:7113
```

### Smart-X Client

```text
https://localhost:7292
```

The browser should open the Smart-X gateway startup interface.

If the browser does not open automatically, navigate to the client address manually.

---

# Application Navigation

The startup interface displays the three Smart-X platform modules.

Select:

```text
Sensor Data Ingestion & Telemetry
```

to enter the Part 1 Operations Dashboard.

The management workspace provides access to:

```text
Dashboard
Sensors
Telemetry
Attention Centre
Attachments
Deployment Validation
```

---

# Testing the Application

The following sequence can be used to verify the main Smart-X functionality.

## 1. Verify Startup Data

After starting the API, confirm that the gateway contains:

```text
24 registered sensors
6,000 telemetry packets
```

---

## 2. Test Sensor Registration

Open:

```text
Sensors
```

Register a new device with:

- Device identifier
- Deployment location
- Sensor category

Confirm that:

- The sensor appears in the registry.
- The sensor count increases.
- Duplicate identifiers are rejected.
- Delete confirmation is displayed before removing a sensor.

---

## 3. Test Telemetry Ingestion

Open:

```text
Telemetry
```

Select:

- Telemetry type
- Registered sensor
- Metric
- Unit
- Value

Submit the reading.

Confirm that:

- The gateway accepts the reading.
- The telemetry count increases.
- The new reading appears in the telemetry stream.

---

## 4. Test Telemetry Filtering

Use the Telemetry Stream filters to search by:

- Device identifier
- Telemetry type
- Metric

Confirm that the displayed readings update according to the selected filters.

---

## 5. Test Power Analysis

Enter values for:

```text
Meter A
Meter B
```

Select:

```text
Analyse load
```

Confirm that Smart-X returns:

- Combined load
- Load difference
- Highest-load meter

---

## 6. Test Anomaly Detection

Submit a telemetry reading that exceeds one of the configured thresholds.

Examples include:

```text
Soil Moisture < 20%
Temperature > 38 °C
Humidity < 25%
Power Draw > 4500 W
Energy Usage > 5500 Wh
Flow Rate > 350 L/h
```

Open the Attention Centre and confirm that the corresponding alert is displayed.

---

## 7. Test Alert Acknowledgement

Open:

```text
Attention Centre
```

Select an unacknowledged alert and acknowledge it.

Confirm that:

- The alert is marked as acknowledged.
- The active Attention Required count decreases.
- The alert remains part of the overall alert collection.

---

## 8. Test Sensor Attachments

Open:

```text
Attachments
```

1. Select a registered sensor.
2. Choose a supported file.
3. Upload the file.
4. Confirm the successful upload message.
5. Confirm the attachment appears under the selected sensor.

The encrypted physical file can be found locally under:

```text
SmartX.Api/EncryptedUploads/
```

The stored file should not contain the original readable plaintext content.

---

## 9. Test Deployment Validation

Open:

```text
Deployment Validation
```

Submit a valid nested deployment hierarchy and confirm that the gateway reports it as valid.

Then mark a nested node as incorrectly configured and validate again.

The complete hierarchy should be reported as invalid because recursive validation reaches the incorrectly configured child node.

---

# HTTP Response Behaviour

The API uses standard HTTP responses where appropriate.

Examples include:

| Status | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `201 Created` | Resource successfully created |
| `202 Accepted` | Telemetry accepted for ingestion |
| `400 Bad Request` | Invalid request or validation failure |
| `404 Not Found` | Requested resource does not exist |
| `409 Conflict` | Duplicate resource conflict |

---

# Important Development Notes

## In-Memory Data

The Part 1 Smart-X gateway uses in-memory collections for the simulated sensor and telemetry environment.

Restarting the API recreates the seeded Smart-X workload.

This means runtime actions such as additional telemetry submissions and alert acknowledgements are reset when the API restarts.

---

## Mock Telemetry

Physical IoT hardware is not required to run the application.

The gateway automatically creates the simulated sensor estate and telemetry workload during startup.

---

## Local HTTPS

The development environment uses HTTPS.

If the local .NET development certificate is not trusted, it can be configured using:

```bash
dotnet dev-certs https --trust
```

---

## Encryption Secret

The attachment encryption key is environment-specific and is not included in the repository.

A valid key must be configured through .NET User Secrets before encrypted attachment functionality is used.

---

## Encrypted Uploads

The following directory contains runtime-generated encrypted attachments:

```text
SmartX.Api/EncryptedUploads/
```

It is intentionally excluded from Git source control.

---

# Smart-X Part 1 Summary

Smart-X Part 1 implements a functional IoT Data Ingestion and Validation Gateway using .NET 10.

The solution demonstrates:

- ASP.NET Core Web API development
- Blazor WebAssembly frontend development
- RESTful client/server communication
- Generic programming
- Strongly typed IoT telemetry
- Operator overloading
- Jagged arrays
- Generic collections
- Recursive algorithms
- High-volume mock telemetry
- Sensor registration
- Telemetry ingestion
- Telemetry filtering
- Anomaly detection
- Sensor connectivity monitoring
- Operator alert acknowledgement
- Secure file uploads
- Encrypted file storage
- Responsive IoT management interfaces

The architecture provides the foundation for the additional Smart-X functionality to be introduced during subsequent POE stages.

---

## Developer

**Raghav Mahraj**  
**Student Number:** ST10458336  
**Module:** PROG7312  
**Project:** Smart-X IoT Mesh Ecosystem — POE Part 1
