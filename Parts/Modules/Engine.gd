extends PartModule
# Search !TMP for all temporary solutions.

""" [Chitak]
Links:
	Delta V calculation: https://forum.kerbalspaceprogram.com/topic/39995-questions-about-rocket-equations-isp-exhaust-velocity-and-tw-ratios/#:~:text=The%20rocket%20equation%20is%20Delta,it's%20velocity%20divided%20by%20acceleration.
	Important stuff: https://courses.lumenlearning.com/suny-physics/chapter/8-7-introduction-to-rocket-propulsion/
"""

# !TMP: Hardcoded values are used for now, get them from the part config or whatever
const gasVelocity:float = 0
const gasMass:float = 0
const gasEjectionTime:float = 0

var thrust:float = 0  # Calculated, not used (maybe for display purposes about the engine idk)
var isp:int = 0  # Calculated, not used (maybe for display purposes about the engine idk)

# !TMP: Hardcoded values are used for now, maybe replace with the reference to the craft script to modify its variable instead
# Or just iterate through all of these part modules and collect these values
var acceleration:float = 0

# [Chitak] Constantly fetch these values or just get them off of something else
var rocketMass:float = 0
var gravity:float = 0

func _process(_delta):
	# [Chitak] NERD STUFF (i'm glad i studied rocket physics in middle school)
	acceleration = ((gasVelocity/rocketMass)*(gasMass/gasEjectionTime))-gravity
	thrust = gasVelocity*(gasMass/gasEjectionTime)
	isp = gasVelocity/gravity
