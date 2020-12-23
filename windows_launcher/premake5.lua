local _DependencyFolder = path.getabsolute("libs")
function DependencyFolder()
	return path.getrelative(os.getcwd(), _DependencyFolder)
end

workspace "BanGround"
	location "./build"
	objdir "%{wks.location}/obj"
	targetdir "%{wks.location}/bin/%{cfg.platform}/%{cfg.buildcfg}"

	architecture "x64"
	platforms "x64"

	configurations {
		"Debug",
		"Release",
	}

	buildoptions "/std:c++latest"
	systemversion "latest"
	symbols "On"
	staticruntime "On"
	editandcontinue "Off"
	warnings "Off"
	characterset "Unicode"

	flags {
		"NoIncrementalLink",
		"NoMinimalRebuild",
		"MultiProcessorCompile",
	}

	staticruntime "Off"

	configuration "Release"
		optimize "Full"
		buildoptions "/Os"

	configuration "Debug"
		optimize "Debug"

	project "Launcher"
		targetname "BanGround"

		language "C++"
		kind "WindowedApp"

		files {
			"./src/**.hpp",
			"./src/**.cpp",
			"./src/**.rc",
			"./src/resources/**.*",
		}

		includedirs {
			"%{prj.location}/src",
		}

		libdirs { 
			"libs" 
		}

		resincludedirs {
			"$(ProjectDir)src"
		}

		links { 
			"UnityPlayerStub",
		}

		configuration "Release"
			linkoptions "/SAFESEH:NO /LARGEADDRESSAWARE"
			syslibdirs {
				"./libs/Release",
			}

		configuration "Debug"
			linkoptions "/SAFESEH:NO /LARGEADDRESSAWARE"
			syslibdirs {
				"./libs/Debug",
			}
