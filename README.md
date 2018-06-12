# Forecast 

Forecast is small and simple cross-platform utility for testing HTTP and WebSocket server connectivity. With this tool you can do the following.

- Test if a host is able to receive connections
- Check for and validate certificates remotely 
- Test both IPv4 and IPv6
- Validate the existence of A and AAAA records
- Human readable outputs

# Downloads

Click [here](https://github.com/RainwayApp/Forecast/releases/latest) 

# Usage 


```
 -h, --host         Required. The host you wish to verify.

 -p, --port         Required. The port the host is listening on.

 -f, --family       Required. The address family you wish to verify.

 -b, --beautiful    (Default: true) Indicates if the output should be pretty printed.

 --help             Display this help screen.

 --version          Display version information.
 
```


## Examples


### Check a host over IPv6

```./Forecast -h rainway.io -p 443 -f ipv6```

```json
{
  "Cert": {
    "CertValid": true,
    "Subject": {
      "CN": "ssl392138.cloudflaressl.com",
      "OU": "Domain Control Validated"
    },
    "Issuer": {
      "CN": "COMODO ECC Domain Validation Secure Server CA 2",
      "O": "COMODO CA Limited",
      "L": "Salford",
      "S": "Greater Manchester",
      "C": "GB"
    }
  },
  "CouldPing": true,
  "HasTargetAddressFamily": true,
  "HostName": "rainway.io",
  "IpAddress": "2400:cb00:2048:1::6818:74c",
  "Port": 443,
  "AddressFamily": "InterNetworkV6"
}
```

### Check a host over IPv4
```./Forecast -h google.com -p 80 -f ipv4```



```
{
  "Cert": null,
  "CouldPing": true,
  "HasTargetAddressFamily": true,
  "HostName": "google.com",
  "IpAddress": "172.217.3.206",
  "Port": 80,
  "AddressFamily": "InterNetwork"
}
```