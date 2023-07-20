# HeightsOfSkyrim-DiverseRacialSkeletons
A Synthesis patcher for Skyrim that tries to make the best of both worlds between FK's Racial Skeletons and Heights of Skyrim, while also ensuring that both changes are present, regardless of the rest of your load order.

As an added bonus, this patcher ensures that the filepath to the racial skeleton from FK's is correctly applied. (If you have a any extra race editing mods this would normally be overwritten by them.)

### Justification
I like the NPC variance of Heights of Skyrim, but more-so than that, I enjoy the skeleton changes from FK's. The problem is that both of these mods attempt to accomplish the same thing, just via different methods. HoS is at the NPC level, whereas FK's is at the race level. When compounded together, the result is...egregious.

This patch gives you the ability to "reign in" Heights of Skyrim NPC height changes to be less pronounced when compared to vanilla.

### Known Issues
* If the NPC has a base height other than 1.0 within the base game, this patcher will produce incorrect results. I'm not immediately aware of any NPCs that meet this criteria, but I haven't looked very hard.
* When run alongside facefixer synthesis patch (which everyone should use), ensure that this patcher is LOWER than that patch, otherwise it will overwrite your changes. You can open your synthesis patch in xEdit to make sure it's working correctly.
