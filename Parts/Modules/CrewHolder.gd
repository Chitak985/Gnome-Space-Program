extends PartModule
# Search !TMP for all temporary solutions.

# !TMP: Hardcoded values are used for now, get them from the part config or whatever
var maxCapacity:int = 0

var currentCrew:int = 0  # Modified per instance

# !TMP: Hardcoded values are used for now, maybe replace with the reference to the craft script to modify its variable instead
# Or just iterate through all of these part modules and collect these values
var gForce:float = 0

func _process(_delta):
	if(gForce > 5):
		pass  # Make the crew lose consciousness or something
