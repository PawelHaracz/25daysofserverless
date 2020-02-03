# Security on Azure functions

## What was done

1. Enable authentication, uses facebook authentication. [describe how works here](https://pawelharacz.com/2019/12/30/authentication-in-azure-functions/)
1. KeyVault integration. [image here](https://github.com/PawelHaracz/25daysofserverless/blob/master/Day24/Pictures/KeyVaultIntegration.jpg)
    - Create dedicated KeyVault
    - Enable MSI
    - uses OOB integration with kv and function
1. Host Web application on azure blob storage
1. Enable CORS policy [imge here](https://github.com/PawelHaracz/25daysofserverless/blob/master/Day24/Pictures/CORS.jpg)
    - Allow only request from blob storage url
    - Allows Credentials enable
1. Allows only HTTPS connections [image here](https://github.com/PawelHaracz/25daysofserverless/blob/master/Day24/Pictures/HTTPs.jpg)
    - https only
    - Miminal TLS 1.2
    - enable Incomming client certificates

## How to configure it

1. Enable authentication
    - Go to platform features
    - Open Authentication section
    - Toggle on App Service Authentication
    - Select Log in with Facebook in Action to take when requset is not authenticated
    - Configure facebook provider
        - provide App ID and App Secret
        - select email and public_profie scope
    - Toggle off token store
    - Provide Allowed External Redirect URL
        - your app url
    - Save
1. Key Vault integration
    - Go to Identity section
    - Toggle on Status
    - Save
    - Create Key Vault
    - Add secretes
    - Create Access policy for Azure function
        - Assing their List and Get privilage for secrets
    - Save
    - Open Azure function configuration
    - Fill all confiuration with reference to keyVault
        - @Microsoft.KeyVault(SecretUri=<KeyVaultName>.vault.azure.net/secrets/<your-secret-name>/)
    - Save
1. CORS
    - Go CORS section
    - Enable CORS
    - Enable Access-Control-Allow-Credentials
    - Add Frontend page to allowed origins
    - Save
1. Allows only HTTPS
    - Go to SSL section
    - Toogle on Https only
    - Toogle on minimal LTS 1.2
    - Toogle on incomming client certificates
