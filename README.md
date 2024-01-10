# OpenTelemetry Auto-Instrumentation Demo

## I. Install the Sample Application on IIS

1. **Clone the repository:**
   
    ```bash
    git clone https://github.com/tripvik/OtelTestApp
    ```

2. **Publish the `OtelTestApp` and `SampleGrpcService` on IIS.**
   
    Ensure it is configured for HTTPS since HTTP/2 is not supported over HTTP in IIS.

3. **Update the gRPC and Redis endpoints in the `appsettings.json` of the deployed `OtelTestApp`.**

## II. Configure OpenTelemetry Auto-Instrumentation
Run the following PowerShell commands in admin mode to install the Auto-Instrumentation.

```powershell
# Download Scripts
$module_url = "https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/latest/download/OpenTelemetry.DotNet.Auto.psm1"
$download_path = Join-Path $env:temp "OpenTelemetry.DotNet.Auto.psm1"
Invoke-WebRequest -Uri $module_url -OutFile $download_path -UseBasicParsing

# Install Modules
Import-Module $download_path
Install-OpenTelemetryCore

# Instrument IIS
Register-OpenTelemetryForIIS
```

## III. Configure and Run OpenTelemetry Collector

1. **Navigate to the CNAO tenant (https://<tenant_name>.observe.appdynamics.com/ui/configure/kubernetes-and-apm) and generate OAuth credentials.**

2. **Download the Otel Collector for Windows from [here](https://github.com/open-telemetry/opentelemetry-collector-releases/releases).**

3. **Update the credentials in the collector configuration `otel-config.yaml` located in the config folder .**

4. **Start `otelcol-contrib.exe` using the following command:**

    ```bash
    otelcol-contrib.exe --config=otel-config.yaml
    ```

   Optionally, you may install it as a service using the following command:

    ```bash
    sc.exe create "OpenTelemetry Collector Service" displayname= "OpenTelemetry Collector Service" start= delayed-auto binPath= "\"<Replace with your path>\\otelcol-contrib.exe\" --config=\"<Replace with your path>\\otel-config.yaml\""
    ```
   
   Make sure to replace `<Replace with your path>` with the actual path on your system.

5. **Run Zipkin:**
    Make sure If you have Java 8 or higher installed. Download the latest release as a self-contained executable jar [here](https://search.maven.org/remote_content?g=io.zipkin&a=zipkin-server&v=LATEST&c=exec):
    
    Run the following command sto start zipkin.
    ```bash
    java -jar zipkin.jar
    ```

6. **Generate traffic on the IIS application.**

## IV. Analyze the Telemetry

1. **Review the traces in Zipkin by navigating to [http://localhost:9411/zipkin/](http://localhost:9411/zipkin/).**

2. **Review the telemetry in CNAO by applying the following filter on navigating to [https://<tenant_name>.observe.appdynamics.com/ui/observe](https://<tenant_name>.observe.appdynamics.com/ui/observe):**

   ```
   attributes(service.name) = '<iis_site_name>'
   ```
