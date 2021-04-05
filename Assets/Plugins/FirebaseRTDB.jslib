// Code from:
// https://github.com/rotolonico/FirebaseWebGL
mergeInto(LibraryManager.library, {

  PushJSON: function(path, value, objectName, callback, fallback) {
    var parsedPath = Pointer_stringify(path);
    var parsedValue = Pointer_stringify(value);
    var parsedJSON = JSON.parse(parsedValue);
    var parsedObjectName = Pointer_stringify(objectName);
    var parsedCallback = Pointer_stringify(callback);
    var parsedFallback = Pointer_stringify(fallback);

    try {
        firebase.database().ref(parsedPath).push().set(parsedJSON).then(function(unused) {
            window.unityInstance.SendMessage(parsedObjectName, parsedCallback);
        });
    } catch (error) {
        window.unityInstance.SendMessage(parsedObjectName, parsedFallback);
    }
  }

});