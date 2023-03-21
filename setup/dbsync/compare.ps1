# Define the path to the .exe file
$exePath = "C:\\Program Files (x86)\\Red Gate\\SQL Compare 12\\SQLCompare.exe"

# Define the arguments to pass to the .exe file (if any)
$args = "/Server1:{0} /db1:{0} /username1:{0} /password1:{0} /Server2:{0} /db2:{0} /username2:{0} /password2:{0} /force /quiet /Options:iu,incd /exclude:user /logLevel:Verbose "

# if (!string.IsNullOrWhiteSpace(ScriptFile))
# 	sb.AppendFormat("/ScriptFile:\"{0}\" ", ScriptFile);
# if (Synchronize)
# 	sb.AppendFormat("/synchronize ");


# Start the process
Start-Process -FilePath $exePath -ArgumentList $args