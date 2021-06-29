# oci-auto-instance-upgrade

This C# .net 5 micro application will attempt to upgrade an Oracle Cloud compute instance of your choice to another instance shape.

## Why?
Oracle Cloud Infrastructure recently introduced A1 Arm instances in their free tier.
Unfortunately, these instances are oversubscribed, making them difficult to acquire.

This tool is a soluton to the above problem. It will endlessly attempt to reshape your instance, calling the Oracle Cloud API no more than once per second until the operation succeeds.

## Instructions
You will need private credentials stored in ~/.oci for the Oracle Cli to work. See the following guide.
https://docs.oracle.com/en-us/iaas/Content/API/SDKDocs/dotnetsdkgettingstarted.htm

Target instance OCID and desired shape must then be configured in appsettings.json.
```
cp appsettings.example.json appsettings.json
dotnet run --release
```

