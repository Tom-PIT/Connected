#!/bin/bash

echo "Modify sys/sys.json"
jq --arg my_var "$sys_config_db_connection" '.connectionstrings.sys = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json
jq --arg my_var "$sys_config_valid_issuer" '.authentication.jwtoken.validissuer = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json
jq --arg my_var "$sys_config_valid_audience" '.authentication.jwtoken.validaudience = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json
jq --arg my_var "$sys_config_issuer_signing_key" '.authentication.jwtoken.issuersigningkey = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json
jq --arg my_var "$sys_storage_provider" '.storageProviders = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json
jq --arg my_var "$sys_database" '.database = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/sys/sys.json > /app/sys/sys.json.new && mv /app/sys/sys.json.new /app/sys/sys.json


echo "Modify mng/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/mng/sys.json > /app/mng/sys.json.new && mv /app/mng/sys.json.new /app/mng/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/mng/sys.json > /app/mng/sys.json.new && mv /app/mng/sys.json.new /app/mng/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/mng/sys.json > /app/mng/sys.json.new && mv /app/mng/sys.json.new /app/mng/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/mng/sys.json > /app/mng/sys.json.new && mv /app/mng/sys.json.new /app/mng/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/mng/sys.json > /app/mng/sys.json.new && mv /app/mng/sys.json.new /app/mng/sys.json

echo "Modify app/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/app/sys.json > /app/app/sys.json.new && mv /app/app/sys.json.new /app/app/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/app/sys.json > /app/app/sys.json.new && mv /app/app/sys.json.new /app/app/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/app/sys.json > /app/app/sys.json.new && mv /app/app/sys.json.new /app/app/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/app/sys.json > /app/app/sys.json.new && mv /app/app/sys.json.new /app/app/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/app/sys.json > /app/app/sys.json.new && mv /app/app/sys.json.new /app/app/sys.json

echo "Modify cdn/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/cdn/sys.json > /app/cdn/sys.json.new && mv /app/cdn/sys.json.new /app/cdn/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/cdn/sys.json > /app/cdn/sys.json.new && mv /app/cdn/sys.json.new /app/cdn/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/cdn/sys.json > /app/cdn/sys.json.new && mv /app/cdn/sys.json.new /app/cdn/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/cdn/sys.json > /app/cdn/sys.json.new && mv /app/cdn/sys.json.new /app/cdn/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/cdn/sys.json > /app/cdn/sys.json.new && mv /app/cdn/sys.json.new /app/cdn/sys.json

echo "Modify search/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/search/sys.json > /app/search/sys.json.new && mv /app/search/sys.json.new /app/search/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/search/sys.json > /app/search/sys.json.new && mv /app/search/sys.json.new /app/search/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/search/sys.json > /app/search/sys.json.new && mv /app/search/sys.json.new /app/search/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/search/sys.json > /app/search/sys.json.new && mv /app/search/sys.json.new /app/search/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/search/sys.json > /app/search/sys.json.new && mv /app/search/sys.json.new /app/search/sys.json

echo "Modify iot/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/iot/sys.json > /app/iot/sys.json.new && mv /app/iot/sys.json.new /app/iot/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/iot/sys.json > /app/iot/sys.json.new && mv /app/iot/sys.json.new /app/iot/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/iot/sys.json > /app/iot/sys.json.new && mv /app/iot/sys.json.new /app/iot/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/iot/sys.json > /app/iot/sys.json.new && mv /app/iot/sys.json.new /app/iot/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/iot/sys.json > /app/iot/sys.json.new && mv /app/iot/sys.json.new /app/iot/sys.json

echo "Modify worker/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/worker/sys.json > /app/worker/sys.json.new && mv /app/worker/sys.json.new /app/worker/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/worker/sys.json > /app/worker/sys.json.new && mv /app/worker/sys.json.new /app/worker/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/worker/sys.json > /app/worker/sys.json.new && mv /app/worker/sys.json.new /app/worker/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/worker/sys.json > /app/worker/sys.json.new && mv /app/worker/sys.json.new /app/worker/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/worker/sys.json > /app/worker/sys.json.new && mv /app/worker/sys.json.new /app/worker/sys.json

echo "Modify rest/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/rest/sys.json > /app/rest/sys.json.new && mv /app/rest/sys.json.new /app/rest/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/rest/sys.json > /app/rest/sys.json.new && mv /app/rest/sys.json.new /app/rest/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/rest/sys.json > /app/rest/sys.json.new && mv /app/rest/sys.json.new /app/rest/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/rest/sys.json > /app/rest/sys.json.new && mv /app/rest/sys.json.new /app/rest/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/rest/sys.json > /app/rest/sys.json.new && mv /app/rest/sys.json.new /app/rest/sys.json

echo "Modify dev/sys.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/dev/sys.json > /app/dev/sys.json.new && mv /app/dev/sys.json.new /app/dev/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/dev/sys.json > /app/dev/sys.json.new && mv /app/dev/sys.json.new /app/dev/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/dev/sys.json > /app/dev/sys.json.new && mv /app/dev/sys.json.new /app/dev/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/dev/sys.json > /app/dev/sys.json.new && mv /app/dev/sys.json.new /app/dev/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/dev/sys.json > /app/dev/sys.json.new && mv /app/dev/sys.json.new /app/dev/sys.json

echo "Modify bigdata/bigdata.json"
jq --arg my_var "$config_connection_name" '.connections[0].name = $my_var' /app/bigdata/sys.json > /app/bigdata/sys.json.new && mv /app/bigdata/sys.json.new /app/bigdata/sys.json
jq --arg my_var "$config_connection_url" '.connections[0].url = $my_var' /app/bigdata/sys.json > /app/bigdata/sys.json.new && mv /app/bigdata/sys.json.new /app/bigdata/sys.json
jq --arg my_var "$config_connection_auth" '.connections[0].authenticationToken = $my_var' /app/bigdata/sys.json > /app/bigdata/sys.json.new && mv /app/bigdata/sys.json.new /app/bigdata/sys.json
jq --arg my_var "$config_plugins_path" '.plugins.location = $my_var' /app/bigdata/sys.json > /app/bigdata/sys.json.new && mv /app/bigdata/sys.json.new /app/bigdata/sys.json
jq --arg my_var "$config_plugins_shadow_copy" '.plugins.shadowCopy = $my_var' /app/bigdata/sys.json > /app/bigdata/sys.json.new && mv /app/bigdata/sys.json.new /app/bigdata/sys.json

echo "Start apps"
exec /app/start-apps.sh