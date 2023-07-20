# HeightsOfSkyrim-DiverseRacialSkeletons
A Synthesis patcher for Skyrim that tries to make the best of both worlds between FK's Racial Skeletons and Heights of Skyrim, while also ensuring that both changes are present, regardless of the rest of your load order.

### Justification
I like the NPC variance of Heights of Skyrim, but more-so than that, I enjoy the skeleton changes from FK's. The problem is that both of these mods attempt to accomplish the same thing, just via different methods. HoS is at the NPC level, whereas FK's is at the race level. When compounded together, the result is...egregious.

This patch gives you the ability to "reign in" Heights of Skyrim NPC height changes to be less pronounced when compared to vanilla.

### Known Issues
* If the NPC has a base height other than 1.0, this patcher will produce incorrect results.
* When run alongside facefixer synthesis patch (which everyone should use), ensure that this patcher is LOWER than that patch, otherwise it will overwrite your changes. You can open your synthesis patch in xEdit to make sure it's working correctly.