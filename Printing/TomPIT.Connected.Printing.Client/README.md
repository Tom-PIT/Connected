# Tom PIT Printing Client



## Legal

Copyright (c) 2020-2021 Tom PIT. All rights reserved.

Licensed under GNU Affero General Public License version 3.

Read about Tom PIT licensing here: https://www.tompit.net/legal/open-release-license



## Setting up



### Installation and Uninstallation

To install and uninstall Tom PIT printing client use ```InstallUtil.exe``` utility, which is part of Microsoft.NET. It is usually located in:

- 32 bit: ```%WINDIR%\Microsoft.NET\Framework\<version>\InstallUtil.exe```
- 64 bit: ```%WINDIR%\Microsoft.NET\Framework64\<version>\InstallUtil.exe```



```%WINDIR%``` is Windows Installation Folder (usually ```C:\Windows```), version is library version (e.g. ```v4.0.30319```), so the example path would be ```C:\Windows\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe```



#### Installation

From command line (```cmd``` or ```PowerShell```) run the following command:

```InstallUtil TomPIT.Connected.Printing.Client.exe```



#### Uninstallation

```InstallUtil /u TomPIT.Connected.Printing.Client.exe```



## Configuration

```xml
<appSettings>
	<add key="cdnUrl" value="http://server/cdn/" />
    <add key="token" value="0f69745a-c65f-437b-a025-5ed52e5dca72" />
    <add key="availablePrinters" value="HPE9367C (HP Officejet Pro 8620)" />
    <add key="loggingLevel" value="error" />
    <add key="exceptionLoggingLevel" value="errorMessage" />
    <add key="printerNameMappings" value="Beautiful printer=HPE9367C (HP Officejet Pro 8620)" />
</appSettings>

<printers>
    <printer name="PrinterName 1" />
    <printer name="PrinterName 2" />
    <printer name="PrinterName 3" />
</printers>

<printerMappings>
    <mapping resourceName="Printer in Office" printerName="PrinterName 1" />
    <mapping resourceName="Printer in Warehouse" printerName="PrinterName 2" />
</printerMappings>
```

### Keys

#### Section ```appSettings```

- ```cdnUrl``` - URL to TomPIT.Connected CDN server
- ```token``` - token from CDN App registration
- ```availablePrinters``` - list of available printers; this value can be:
  - ```default``` - will register default locally installed printer
  - ```installed``` - will register all locally installed printers
  - ```list``` - will register printers from list supplied in ```<printers>``` section
  - semi-colon separated list of printer names [^1] (e.g. ```Printer 1;Printer 2;Some network Printer```)
- ```loggingLevel``` - level of information, that will be logged to log file; available values are:
  - ```off``` - no logging will be done
  - ```fatal``` - only fatal errors will be logged
  - ```error``` - errors and fatal errors will be logged
  - ```warning``` - log warnings
  - ```info``` - log all information
  - ```debug``` - log information that can be useful when searching for errors
  - ```trace``` - extensive logging
  - ```all``` - log everything
- ```exceptionLoggingLevel``` - level of information, that will be logged to log file when exception is thrown; available values are:
  - ```off``` - no exception logging will be done
  - ```errorMessage``` - log only error messages
  - ```stackTrace``` - with error message log also stack-trace
- ```printerNameMappings``` - semi-colon separated list of key-value paired printer names in form of ```friendlyName=printerName``` (e.g. ```Beautiful printer=HPE9367C (HP Officejet Pro 8620)``` which redirects all requests to printer named ```Beautiful printer``` (which is **only** a friendly name) to actual system printer by name ```HPE9367C (HP Officejet Pro 8620)```; this way the user does not have to know all installed printers because we can map some obscure system printer name to e.g. ```Printer in main office```)

[^1]: In case of shared printers the fully qualified name must be used, not just share name (e.g. for shared printer named ```SharedPrinter``` which is shared on computer ```SharePC``` the name provided **must** be ```\\SharePC\SharedPrinter``` and not only ```SharedPrinter```)



#### Section ```printers```

* ```name``` - name of the printer [^1], similar to ```availablePrinters``` in ```appSettings``` section, but it must contain the name of the printer



#### Section ```printerMappings```

* ```resourceName``` - name of the printer in resource management in Tom PIT platform
* ```printerName``` - name of the printer to be mapped; must be in available printers list (```availablePrinters``` setting in ```appSettings``` section or in list in ```printers``` section)

## Logging

Logging is done to folder ```%PROGRAMDATA%\TomPIT\TomPIT.Connected.Printing.Client\logs```. ```%PROGRAMDATA%``` is Windows Special Folder used for sharing between user accounts and in usually located in ```C:\ProgramData```.

There are two types of log files:

- standard logging: files in format ```yyyy_mm_dd.log``` (e.g. ```2020_11_24.log```)
- error logging: files in format ```yyyy_mm_dd.error.log``` (e.g. ```2020_11_24.error.log```)



### Log file content example

```
15:36:57.055|DEBUG|Creating connection to http://server/cdn/printing
15:36:58.189|DEBUG|Print spooler started
15:36:58.194|DEBUG|Registering printers...
15:36:58.289|TRACE|Registered Printers: HPE9367C (HP Officejet Pro 8620)
16:19:05.714|DEBUG|Creating connection to http://server/cdn/printing
16:19:06.195|DEBUG|Print spooler started
16:19:06.200|DEBUG|Registering printers...
16:19:06.288|TRACE|Registered Printers: HPE9367C (HP Officejet Pro 8620)
```

