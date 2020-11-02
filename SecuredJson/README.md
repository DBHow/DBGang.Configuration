## Secure your sensitive data in JSON file for your .NET applications.

This is a direct extension to Microsoft.Extensions.Configuration.Json. It adds feature to protect sensitive data in JSON configuration file.

The previous version (https://www.nuget.org/packages/DBGang.Configuration.SecuredJson/1.1.2) doesn't work well with array and complex types in JSON file. This update fixes the issue.

To encrypt a particular property, add **UNENCRYPTED::** as prefix to the property name, like **UNENCRYPTED::myStringKey**. Note that all letters in **UNENCRYPTED::** are capital case followed by two colons. After the json file was initially loaded, properties prefixed with this special mark would be encrypted. The prefix was changed from **UNENCRYPTED::** to **ENCRYPTED::** to indicate the corresponding value field has been encrypted. Below shows how a json file looks like before and after initial load:

```
{
  "ObjKey": {
    "UNENCRYPTED::SubStringKey1": "SubStringValue1",
    "SubIntegerKey2": 20,
    "UNENCRYPTED::SubBooleanKey3": true,
    "UNENCRYPTED::SubArray1": [
      "SubItem1",
      "SubItem2",
      "SubItem3"
    ]
  },
  "UNENCRYPTED::ArrayKey": [
    "Item1",
    "Item2",
    "Item3"
  ],
  "UNENCRYPTED::StringKey1": "StringValue1",
  "IntegerKey2": 2,
  "BooleanKey3": false,
  "UNENCRYPTED:ConnectionStrings": {
    "MyDB": "Server=myServer;Database=myDatabase;User Id=myUserId;Password=myPassword;"
  }
}
```

```
{
  "ObjKey": {
    "ENCRYPTED::SubStringKey1": "hRXkhRhm1yeUO4YCAeQ6sKiOGwrDC1Lj3pFDk7kEYDw=",
    "SubIntegerKey2": 20,
    "ENCRYPTED::SubBooleanKey3": "UIQ2ZjWN+2TFmW0xCEoxXQ==",
    "ENCRYPTED::SubArray1": "17cAy+xiWZraHtApkmq1q69xHcmL5mAiTYm78dAbc5Gig9b3mK2ngQSqKMxvbr+IcAwz2Xptk0wrj/xXXoqZfw=="
  },
  "ENCRYPTED::ArrayKey": "z96OFCk16fH/Cx5lX3JSAB+UZX+mJCuhA4I0lFFZc6nB352OBd79Q9FUSH6NCb0W",
  "ENCRYPTED::StringKey1": "5SPhANlc4qeIOK0bCWoCjj4M+fcQes9N+3sw6EHsKQY=",
  "IntegerKey2": 2,
  "BooleanKey3": false,
  "ENCRYPTED::ConnectionStrings": "Ej/NOYEk8Yyl29h8AahkJ9/pbuFALcLLvIXy4UqTi3QUJlSEwsK2kyc5iZBgvpJkUzU7M4OhBVJMFvrXE+KbdcR2hPzNsy8WXv3LFsm06FNT3rRyz+pdoDukkd+BiZ5idXdFDyoJHg+Gga3msJ6fiw=="
}
```

Please take a look at the test project to see how to use it.
