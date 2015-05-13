# JsonReader
Minimalistic json parser for C# (one module with no dependencies).

Example:
```json
{
	"sounds": [
    {
	    "name": "coin",
			"file-name": "coin.wav"
		},
		{
			"name": "spikes_show",
			"file-name": "spikes_show.wav"
		},
		{
			"name": "spikes_hide",
			"file-name": "spikes_hide.wav"
		}
	]
}
```
```c#
var jr = new JsonReader(String.Format(@"Content/Sounds/{0}", fileName));

foreach (var sound in jr["sounds"].ToListOfObjects())
{
  var soundName = sound["name"].ToString().Replace("\"", "");
  var soundFileName = sound["file-name"].ToString().Replace("\"", "");
  
  ...
```
