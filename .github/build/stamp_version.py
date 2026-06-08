import json, os, sys

path = ".github/build/installer/Update.json"
version = os.environ["VERSION"]

with open(path) as f:
    data = json.load(f)

if any(e.get("version") == version for e in data):
    print("Update.json already contains entry for " + version + " - skipping.")
    sys.exit(0)

data.append({"version": version, "content": "-- version bump " + version})

with open(path, "w") as f:
    json.dump(data, f)

print("Appended no-op entry for " + version + " to Update.json.")
