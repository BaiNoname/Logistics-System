const { withAndroidManifest } = require('@expo/config-plugins');

module.exports = function withAndroidManifestFix(config) {
  return withAndroidManifest(config, (config) => {
    const application = config.modResults.manifest.application[0];
    application.$['tools:replace'] = 'android:enableOnBackInvokedCallback';
    if (!config.modResults.manifest.$['xmlns:tools']) {
      config.modResults.manifest.$['xmlns:tools'] = 'http://schemas.android.com/tools';
    }
    return config;
  });
};
