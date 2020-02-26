# Thycotic.UiPath.SecureStore
Credential Store plugins for UiPath orchestrator.  To use these credential store plugins, drop the corresponding dll into the plugins directory in the UiPath Orchestrator install directory and modify the web.config as specified here: https://github.com/UiPath/Orchestrator-CredentialStorePlugins

# Secret Server
The Secret Server integration is read only. When an asset or robot is created in UiPath Orchestrator it is linked to a pre-existing secret using the Secret Id.

The integration uses the Secret Server SDK, which is documented in more detail here: https://github.com/thycotic/sdk-documentation

## Setup
In Secret Server:
* Create a new Application Account under Admin -> Users.
* Setup a new onboarding rule in the SDK Client Management under Admin -> See All -> Tools and Integrations
  * Note the onboarding rule name and key
In Orchestrator:
* Add a new Credential Store under the Credential Stores menu under your profile.
  * Secret Server URL - The URL of your secret server instance
  * Rule Name - The client onboarding rule name
  * Rule Key - The optional key from the onboarding rule
  * Reset Key - Random text used to revoke the Secret Server SDK and reinitalize it.
  * Username Field Slug - The slug name of the Secret Template field that Orchestrator will pull the username from when retrieving an Asset from Secret Server.
  * Password Field Slug - The slug name of the Secret Tempalte field that Orchestrator will pull the password from when retrieving an Asset from Secret Server.
  * SDK Config Storage Path - When initialized, the Secret Server SDK creates encrypted storage files. These by default are stored in the user's profile directory. If the orchestrator IIS App Pool is not set to Load User Profile, then that path is the Windows temp directory. If the IIS App Pool is set to Load User Profile, you can change this to the app pool identity's profile path. Or you create a custom path and grant permissions to the app pool identity.
* Create an asset of type Credential and select the Secret Server Credential Store. For the External Name enter the Secret Id of an existing Secret you want to pull the username / password from.
  * When the asset is used in a robot process, it will pull the username and password from the Secret.
* Create a robot and select the Secret Server Credential Store. Enter the username and use the Secret Id as the External Name.
  * Now when the robot logs in it will pull the password from the field on the corresponding Secret.


## Notes
* The Application User linked to the client SDK rule must have permissions to the Secrets accessed by UiPath, assign it to a group and grant that group access to the required folders, or grant it explicit access to the Secrets.
* Ensure that the orchestrator machine trusts the TLS certificate used by Secret Server

# Development

To build the plugin DLL's open the solution in Visual Studio and build the project in release mode. Release builds use ILRepack to bundle the dependent libraries into a single file to avoid version conflicts when Orchestrator loads the DLL. This is defined as a target in the SecretServer.SecureStore .csproj file.
