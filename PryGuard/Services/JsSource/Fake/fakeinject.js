if (!window.injected)
    (function () {
        window.injected = true;
        let fakeProfile = {};

        'use strict';

        function addToConsole(s) {
            // addToConsole(s);
        }

        function injectMediaDevices(config) {
            var mediadevicesSettings = fakeProfile.MediaDevicesSettings;
            if (!mediadevicesSettings.HideDevices) {
                return;
            }
            if (!navigator.mediaDevices) {
                return;
            }

            var devices = [];
            for (var index in mediadevicesSettings.Devices) {
                let d = mediadevicesSettings.Devices[index];
                let device = {
                    deviceId: d.deviceId,
                    kind: d.kind,
                    label: d.label,
                    groupId: d.groupId
                }
                device.__proto__ = InputDeviceInfo.prototype;
                devices.push(device);
            }
            Object.defineProperty(navigator.mediaDevices, "enumerateDevices", {
                "value": function () {
                    return Promise.resolve(devices);
                }
            });
        };

        function injectWebRTC(config) {
            var rtcSettings = fakeProfile.WebRtcSettings;
            if (rtcSettings.WebRtcStatus !== 'MANUAL') {
                return;
            }
            var remoteIp = rtcSettings.PublicIp;
            var localIp = rtcSettings.LocalIp;
            let ipRegexp = /^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})/;
            let isNotLocalIp = function (ip) {
                return !ip.match(/^(192\.168\.|169\.254\.|10\.|127\.|172\.(1[6-9]|2\d|3[01])\.)/);
            }
            let isIP = function (ip) {
                return ip.match(ipRegexp);
            }
            let replaceIP = function (line) {
                var e = line.split(" ");
                for (var i = 0; i < e.length; i++) {
                    if (isIP(e[i])) {
                        e[i] = e[i].replace(ipRegexp, isNotLocalIp(e[i]) ? remoteIp : localIp);
                    }
                }
                return e.join(" ");
            }

            function makeid(length) {
                var result = '';
                var characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
                var charactersLength = characters.length;
                for (var i = 0; i < length; i++) {
                    result += characters.charAt(Math.floor(Math.random() * charactersLength));
                }
                return result;
            }

            let randInd = function (min, max) {
                min = Math.ceil(min);
                max = Math.floor(max);
                return Math.floor(Math.random() * (max - min)) + min; // Maximum does not turn on, minimum turns on
            }
            let newCandidate = function () {
                let candstr = "candidate:" + randInd(800000000, 900000000) + " 1 udp " + randInd(1043278322, 2043278322) + " " + remoteIp + " " + randInd(5000, 5600) + " typ srflx raddr 0.0.0.0 rport 0 generation 0 ufrag " + makeid(4) + " network-cost 999";
                return new RTCIceCandidate({
                    sdpMLineIndex: 0,
                    candidate: candstr,
                    sdpMid: "0",
                    usernameFragment: makeid(4)
                });
            }
            var spoof = {
                "candidate": function (target) {
                    if (!target) {
                        return;
                    }
                    var addr = target.prototype.__lookupGetter__('address');
                    var cand = target.prototype.__lookupGetter__('candidate');
                    var typeGetter = target.prototype.__lookupGetter__('type');
                    Object.defineProperties(target.prototype,
                        {
                            address: {
                                get: function () {
                                    var realAddr = addr.call(this);
                                    var type = typeGetter.call(this);
                                    return realAddr.replace(ipRegexp, isNotLocalIp(realAddr) ? remoteIp : localIp);
                                }
                            },
                            candidate: {
                                get: function () {
                                    var realCandidate = cand.call(this);
                                    var type = typeGetter.call(this);
                                    return replaceIP(realCandidate);
                                }
                            },
                        });
                }, "connection": function (target) {
                    if (!target) {
                        return;
                    }
                    var croffer = target.prototype.createOffer;
                    var addcand = target.prototype.addIceCandidate;
                    // var offer = target.prototype.onicecandidate;
                    Object.defineProperty(target.prototype, "createOffer", {
                        configurable: false, enumerable: true, writable: true,
                        "value": function () {
                            let cr = croffer.call(this);
                            // addcand.call(this, newCandidate());
                            if (this.onicecandidate)
                                this.onicecandidate(new RTCPeerConnectionIceEvent("icecandidate", {candidate: newCandidate()}));
                            return cr;
                        }
                    });
                },
                "sdp": function (target) {
                    var g = target.prototype.__lookupGetter__('sdp');
                    Object.defineProperties(target.prototype,
                        {
                            sdp: {
                                get: function () {
                                    var realSdp = g.call(this);
                                    var lines = realSdp.split("\n");
                                    for (var i = 0; i < lines.length; i++) {
                                        if (0 === lines[i].indexOf("a=candidate:")) {
                                            lines[i] = replaceIP(lines[i]);
                                        }
                                    }
                                    return lines.join("\n");
                                }
                            }
                        });
                },
                "iceevent": function (target) {
                    var istrusted = target.prototype.__lookupGetter__('isTrusted');
                    Object.defineProperties(target.prototype,
                        {
                            isTrusted: {
                                get: function () {
                                    return true;
                                }
                            }
                        });
                }
            }
            try {
                spoof.candidate(RTCIceCandidate);
                spoof.connection(RTCPeerConnection);
                // spoof.candidate(webkitRTCPeerConnection);
                spoof.sdp(RTCSessionDescription);
            } catch (e) {
            }
            // spoof.iceevent(RTCPeerConnectionIceEvent);

        }

        var injectWebGl = function (config) {
            var webglstatus = fakeProfile.WebGL.Status;
            if (webglstatus === "OFF") {
                return;
            }
            var webglParams = fakeProfile.WebGL.Params;
            var webglUtil = {
                "random": {
                    "value": function () {
                        return Math.random()
                    },
                    "item": function (e) {
                        var rand = e.length * webglUtil.random.value();
                        return e[Math.floor(rand)];
                    },
                    "array": function (e) {
                        var rand = webglUtil.random.item(e);
                        return new Int32Array([rand, rand]);
                    },
                    "items": function (e, n) {
                        var length = e.length;
                        var result = new Array(n);
                        var taken = new Array(length);
                        if (n > length) n = length;
                        //
                        while (n--) {
                            var i = Math.floor(webglUtil.random.value() * length);
                            result[n] = e[i in taken ? taken[i] : i];
                            taken[i] = --length in taken ? taken[length] : length;
                        }
                        //
                        return result;
                    }
                },
                "spoof": {
                    "webgl": {
                        "buffer": function (target) {
                            const bufferData = target.prototype.bufferData;
                            Object.defineProperty(target.prototype, "bufferData", {
                                "value": function () {
                                    var noise = fakeProfile.WebGL.Noise;
                                    var noiseindex = noise.Index;
                                    var noiseDiff = 0.1 * noise.Difference * arguments[1][noiseindex];
                                    arguments[1][noiseindex] = arguments[1][noiseindex] + noiseDiff;
                                    return bufferData.apply(this, arguments);
                                }
                            });
                        },
                        "parameter": function (target) {
                            const getParameter = target.prototype.getParameter;
                            Object.defineProperty(target.prototype, "getParameter", {
                                configurable: false, enumerable: true, writable: true,
                                "value": function () {
                                    let webglCode = arguments[0];
                                    if (webglParams[webglCode]) {
                                        return webglParams[webglCode].Value;
                                    }

                                    // var float32array = new Float32Array([1, 8192]);
                                    // //
                                    // if (webglCode === 3415) return 0;
                                    // else if (webglCode === 3414) return 24;
                                    // else if (webglCode === 35661) return webglUtil.random.items([128, 192, 256]);
                                    // else if (webglCode === 3386) return webglUtil.random.array([8192, 16384, 32768]);
                                    // else if (webglCode === 36349 || webglCode === 36347) return webglUtil.random.item([4096, 8192]);
                                    // else if (webglCode === 34047 || webglCode === 34921) return webglUtil.random.items([2, 4, 8, 16]);
                                    // else if (webglCode === 7937 || webglCode === 33901 || webglCode === 33902) return float32array;
                                    // else if (webglCode === 34930 || webglCode === 36348 || webglCode === 35660) return webglUtil.random.item([16, 32, 64]);
                                    // else if (webglCode === 34076 || webglCode === 34024 || webglCode === 3379) return webglUtil.random.item([16384, 32768]);
                                    // else if (webglCode === 3413 || webglCode === 3412 || webglCode === 3411 || webglCode === 3410 || webglCode === 34852) return webglUtil.random.item([2, 4, 8, 16]);
                                    // else return webglUtil.random.item([0, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096]);
                                    // //
                                    return getParameter.apply(this, arguments);
                                }
                            });
                        }
                    }
                }
            };
            //
            webglUtil.spoof.webgl.buffer(WebGLRenderingContext);
            webglUtil.spoof.webgl.buffer(WebGL2RenderingContext);
            webglUtil.spoof.webgl.parameter(WebGLRenderingContext);
            webglUtil.spoof.webgl.parameter(WebGL2RenderingContext);
            //
        };

        var inject = function (fakeProfile) {
            const config = {
                'BUFFER': null,
                'getChannelData': function (e) {
                    const getChannelData = e.prototype.getChannelData;
                    Object.defineProperty(e.prototype, 'getChannelData', {
                        'value': function () {
                            const results1 = getChannelData.apply(this, arguments);
                            if (config.BUFFER !== results1) {
                                config.BUFFER = results1;
                                for (var i = 0; i < results1.length; i += 100) {
                                    let index = Math.floor(fakeProfile.ChannelDataIndexDelta * i);
                                    results1[index] = results1[index] + fakeProfile.ChannelDataDelta * 0.0000001;
                                }
                            }
                            return results1;
                        }, onfigurable: false, enumerable: true, writable: true
                    });

                    Object.defineProperty(e.prototype.getChannelData, 'toString', {
                        "value": function () {
                            return 'getChannelData() { [native code] }';
                        }
                    });

                    Object.defineProperty(e.prototype.getChannelData.toString, 'toString', {
                        "value": function () {
                            return 'toString() { [native code] }';
                        }
                    });
                },
                'createAnalyser': function (e) {
                    const createAnalyser = e.prototype.__proto__.createAnalyser;
                    Object.defineProperty(e.prototype.__proto__, 'createAnalyser', {
                        'value': function () {
                            const results2 = createAnalyser.apply(this, arguments);
                            const getFloatFrequencyData = results2.__proto__.getFloatFrequencyData;
                            Object.defineProperty(results2.__proto__, 'getFloatFrequencyData', {
                                'value': function () {
                                    const results3 = getFloatFrequencyData.apply(this, arguments);
                                    for (var i = 0; i < arguments[0].length; i += 100) {
                                        let index = Math.floor(fakeProfile.FloatFrequencyDataIndexDelta * i);
                                        arguments[0][index] = arguments[0][index] + fakeProfile.FloatFrequencyDataDelta * 0.1;
                                    }
                                    return results3;
                                }, configurable: false, enumerable: true, writable: true
                            });

                            Object.defineProperty(results2.__proto__.getFloatFrequencyData, 'toString', {
                                "value": function () {
                                    return 'getFloatFrequencyData() { [native code] }';
                                }
                            });

                            Object.defineProperty(results2.__proto__.getFloatFrequencyData.toString, 'toString', {
                                "value": function () {
                                    return 'toString() { [native code] }';
                                }
                            });

                            return results2;
                        }, configurable: false, enumerable: true, writable: true
                    });
                }
            };

            config.getChannelData(AudioBuffer);
            config.createAnalyser(AudioContext);
            config.createAnalyser(OfflineAudioContext);
            addToConsole('pathed audio');
            //document.documentElement.dataset.acxscriptallow = true;
        };


        /**
         * @type {InjectorOption}
         */
        let config1;

        patchWindow(window, config1);

        pathPlugins(window);
// injectMediaDevices(config1);
        injectWebRTC(config1);

        pathNavigatorСonnection(window);
        pathNavigatorKeyboard(window);
// pathNavigatorPermissions(window);
        patchNavigator(window, config1);
        protectedCanvasFingerPrint(window, config1);
        patchWebgl(window, config1);
        pathAudio(window, config1);
        pathFonts(window, config1);


        iframeContentPatching(config1);


        /**
         *
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function patchWindow($window, config) {
            delete $window.CefSharp;
            delete $window.cefSharp;
            // pathNavigatorPermissions($window);
            patchNavigator($window, config);
        }

        /**
         * @param {Window} $window
         */
        function pathPlugins($window) {
            protectPluginsAndMimetype($window);
            console.info('Plugin patched');

            function protectPluginsAndMimetype($window1) {

                var mime1 = createFakeMimeType('', 'pdf', 'application/pdf');
                var mime2 = createFakeMimeType('Portable Document Format', 'pdf', 'application/x-google-chrome-pdf');
                var mime3 = createFakeMimeType('Native Client Executable', '', 'application/x-nacl');
                var mime4 = createFakeMimeType('Portable Native Client Executable', '', 'application/x-pnacl');
                createFakeMimeTypesInNavigator($window1, [mime1, mime2, mime3, mime4]);


                var plugin1 = createFakePlugin('Portable Document Format', 'internal-pdf-viewer', 'Chrome PDF Plugin');
                var plugin2 = createFakePlugin('', 'mhjfbmdgcfjbbpaeojofohoefgiehjai', 'Chrome PDF Viewer');
                var plugin3 = createFakePlugin('', 'internal-nacl-plugin', 'Native Client');


                setEnebledPlugin(mime1, plugin2);
                setEnebledPlugin(mime2, plugin1);
                setEnebledPlugin(mime3, plugin3);
                setEnebledPlugin(mime4, plugin3);


                updateFakePlugin(plugin1, [mime2]);
                updateFakePlugin(plugin2, [mime1]);
                updateFakePlugin(plugin3, [mime3, mime4]);

                createFakePluginsTypesInNavigator($window1, [plugin1, plugin2, plugin3]);
            }


            function createFakePluginsTypesInNavigator($window1, plugins) {
                var fakeArray = Object.create($window1.navigator.plugins);

                var length = plugins.length;

                for (var i = 0; i < plugins.length; i++) {
                    Object.defineProperty(fakeArray, i, {
                        value: plugins[i], configurable: false, enumerable: true, writable: false
                    });
                    Object.defineProperty(fakeArray, plugins[i].name, {
                        value: plugins[i], configurable: false, enumerable: false, writable: false
                    });
                }

                Object.defineProperty(fakeArray, 'length', {
                    value: length, configurable: false, enumerable: true, writable: false
                });

                Object.defineProperty($window1.navigator, 'plugins', {
                    value: fakeArray, configurable: false, enumerable: true, writable: true
                });
            }

            function createFakeMimeTypesInNavigator($window1, mimeTypes) {
                var fakeArray = Object.create($window1.navigator.mimeTypes);

                var length = mimeTypes.length;
                for (var i = 0; i < mimeTypes.length; i++) {
                    Object.defineProperty(fakeArray, i, {
                        value: mimeTypes[i], configurable: false, enumerable: true, writable: false
                    });
                    Object.defineProperty(fakeArray, mimeTypes[i].type, {
                        value: mimeTypes[i], configurable: false, enumerable: false, writable: false
                    });
                }

                Object.defineProperty(fakeArray, 'length', {
                    value: length, configurable: false, enumerable: true, writable: false
                });

                Object.defineProperty($window1.navigator, 'mimeTypes', {
                    value: fakeArray, configurable: false, enumerable: true, writable: true
                });
            }

            /**
             * Creates the desired plugin
             * @param {string} description
             * @param {string} fileName
             * @param {number} length
             * @param {string} name
             */
            function createFakePlugin(description, fileName, name) {
                var plugin = Object.create(navigator.plugins[0]);

                Object.defineProperty(plugin, 'description', {
                    value: description, configurable: false, enumerable: true, writable: false
                });
                Object.defineProperty(plugin, 'filename', {
                    value: fileName, configurable: false, enumerable: true, writable: false
                });
                //Object.defineProperty(plugin, 'length', {
                //    value: length, configurable: false, enumerable: true, writable: false
                //});

                Object.defineProperty(plugin, 'name', {
                    value: name, configurable: false, enumerable: true, writable: false
                });

                return plugin;
            }

            function updateFakePlugin(plugin, mimeTypes) {

                var length = mimeTypes.length;

                Object.defineProperty(plugin, 'length', {
                    value: length, configurable: false, enumerable: true, writable: false
                });

                for (var i = 0; i < mimeTypes.length; i++) {

                    Object.defineProperty(plugin, i, {
                        value: mimeTypes[i], configurable: false, enumerable: true, writable: false
                    });

                    Object.defineProperty(plugin, mimeTypes[i].type, {
                        value: mimeTypes[i], configurable: false, enumerable: false, writable: false
                    });
                }
            }

            /**
             *
             * @param {string} description
             * @param {string} suffixes
             * @param {string} type
             */
            function createFakeMimeType(description, suffixes, type) {
                var fakeMimeType = Object.create(navigator.mimeTypes[0]);

                Object.defineProperty(fakeMimeType, 'type', {
                    value: type, configurable: false, enumerable: true, writable: false
                });

                Object.defineProperty(fakeMimeType, 'suffixes', {
                    value: suffixes, configurable: false, enumerable: true, writable: false
                });

                Object.defineProperty(fakeMimeType, 'description', {
                    value: description, configurable: false, enumerable: true, writable: false
                });

                return fakeMimeType;
            }

            function setEnebledPlugin(mimeType, plugin) {
                Object.defineProperty(mimeType, 'enabledPlugin', {
                    value: plugin, configurable: false, enumerable: true, writable: false
                });
            }
        }


        /**
         * @param {Window} $window
         */
        function patchChrome($window) {
            function Event() {

            }

            Event.prototype.addListener = function () {
            };
            Event.prototype.addRules = function () {
            };
            Event.prototype.addRules = function () {
            };
            Event.prototype.dispatch = function () {
            };
            Event.prototype.constructor = function () {
            };
            Event.prototype.dispatchToListener = function () {
            };
            Event.prototype.getRules = function () {
            };
            Event.prototype.hasListener = function () {
            };
            Event.prototype.hasListeners = function () {
            };
            Event.prototype.removeListener = function () {
            };
            Event.prototype.removeRules = function () {
            };


            var webStore =
                {
                    install: function (url, onSuccess, onFailure) {
                        console.info('Someone is trying to install an extension');
                    },
                    onDownloadProgress: new Event(),
                    onInstallStageChanged: new Event()
                };
            var app =
                {
                    getDetails: function GetDetails() {
                    },
                    getIsInstalled: function GetIsInstalled() {
                    },
                    installState: function GetInstallState(callback) {
                    },
                    isInstalled: false,
                    runningState: function GetRunningState() {
                    }
                }

            var runtime = {
                OnInstalledReason: {
                    CHROME_UPDATE: 'chrome_update',
                    INSTALL: 'install',
                    SHARED_MODULE_UPDATE: 'shared_module_update',
                    UPDATE: 'update'
                },
                OnRestartRequiredReason: {
                    APP_UPDATE: 'app_update',
                    OS_UPDATE: 'os_update',
                    PERIODIC: 'periodic'
                },
                PlatformArch: {
                    ARM: 'arm',
                    X86_32: 'x86-32',
                    X86_64: 'x86-64'
                },
                PlatformNaclArch: {
                    ARM: 'arm',
                    X86_32: 'x86-32',
                    X86_64: 'x86-64'
                },
                PlatformOs: {
                    ANDROID: 'android',
                    CROS: 'cros',
                    LINUX: 'linux',
                    MAC: 'mac',
                    OPENBSD: 'openbsd',
                    WIN: 'win'
                },
                RequestUpdateCheckStatus: {
                    NO_UPDATE: 'no_update',
                    THROTTLED: 'throttled',
                    UPDATE_AVAILABLE: 'update_available'
                },
                connect: function () {
                    console.info('Someone is trying to use chrome.runtime.connect.');
                },
                sendMessage: function () {
                    console.info('Someone is trying to use chrome.runtime.sendMessage');
                }
            }

            Object.defineProperty(
                app, 'isInstalled',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        return false;
                    },
                });

            Object.defineProperty(
                $window.chrome, 'runtime',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    value: runtime
                });

            Object.defineProperty(
                $window.chrome, 'webstore',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function nativeGetter() {
                        return webStore;
                    }
                });


            Object.defineProperty(
                $window.chrome, 'app',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function nativeGetter() {
                        return app;
                    }
                });

            console.info('Chrome patched');
        };

        /**
         * @param {Window} $window
         */

        function pathNavigatorKeyboard($window) {
            let layoutMap = []
            // Object.defineProperty(
            //     NetworkInformation.prototype, 'downlink',
            //     {
            //         __proto__: null,
            //         configurable: true,
            //         enumerable: true,
            //         get: function () {
            //             return 10;
            //         }
            //     });

        }

        function pathNavigatorСonnection($window) {


            function NetworkInformation() {

            }

            Object.defineProperty(
                NetworkInformation.prototype, 'downlink',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        return 8.1;
                    }
                });

            Object.defineProperty(
                NetworkInformation.prototype, 'effectiveType',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        return '4g';
                    }
                });

            Object.defineProperty(
                NetworkInformation.prototype, 'onchange',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        return null;
                    }
                });

            Object.defineProperty(
                NetworkInformation.prototype, 'rtt',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        return 100;
                    }
                });


            Object.defineProperty(
                NetworkInformation.prototype, 'saveData',
                {
                    __proto__: null,
                    configurable: true,
                    enumerable: true,
                    get: function () {
                        return false;
                    }
                });

            let networkInfo = new NetworkInformation();

            Object.defineProperty(
                $window.navigator, 'connection',
                {
                    __proto__: NetworkInformation.prototype,
                    configurable: true,
                    enumerable: true,
                    value: networkInfo
                });

            console.info('navigator.connection patched');
        }

        /**
         *
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function patchNavigator($window, config) {
            // const newProto = $window.navigator.__proto__;
            // delete newProto.webdriver;
            // $window.navigator.__proto__ = newProto;
            Object.defineProperty(
                $window.navigator, 'deviceMemory',
                {
                    configurable: true,
                    enumerable: true,
                    value: fakeProfile.MemoryAvailable
                });


            Object.defineProperty(
                $window.navigator, 'hardwareConcurrency',
                {
                    configurable: true,
                    enumerable: true,
                    value: fakeProfile.CpuConcurrency
                });
            batteryPath($window);
            console.info('navigator patched');

            var blue = {};
            blue.getAvailability = function () {
                return new Promise(function (resolve, reject) {
                    resolve(false);
                });
            };

            Object.defineProperty(
                $window.navigator, 'bluetooth',
                {
                    configurable: true,
                    enumerable: true,
                    value: blue
                });
        }


        /**
         * @param {Window} $window
         */
        function batteryPath($window) {
            Object.defineProperty($window.BatteryManager.prototype, 'level', {
                value: 1, configurable: false, enumerable: true, writable: true
            });

            Object.defineProperty($window.BatteryManager.prototype, 'dischargingTime', {
                value: Infinity, configurable: false, enumerable: true, writable: true
            });

            Object.defineProperty($window.BatteryManager.prototype, 'charging', {
                value: true, configurable: false, enumerable: true, writable: true
            });

            console.info('BatteryManager patched');
        }

        /**
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function protectedCanvasFingerPrint($window, config) {
            if (!fakeProfile.HideCanvas) {
                return;
            }
            var fillPtr = $window.CanvasRenderingContext2D.prototype.fillRect;

            $window.CanvasRenderingContext2D.prototype.fillRect = function () {
                if (!this.callCounter)
                    this.callCounter = 0;
                this.callCounter++;
                addToConsole('Someone added the text' + JSON.stringify(arguments) + ' ' + this.callCounter);
                return fillPtr.apply(this, arguments);
            };


            var ptr = $window.HTMLCanvasElement.prototype.toDataURL;
            $window.HTMLCanvasElement.prototype.toDataURL = function () {
                if (console) {
                    console.warn('Someone was counting the canvass');
                }
                var ctx = this.getContext('2d');
                if (ctx) {
                    if (ctx.callCounter < 500 || !ctx.callCounter) {
                        ctx.fillStyle = 'rgba(255, 255, 0,0.1)';
                        var randFinger = fakeProfile.CanvasFingerPrintHash;
                        ctx.fillText(randFinger, 2, 15);
                    }
                }
                return ptr.apply(this, arguments);
            }

            // if ($window.location.href.indexOf('wedo.bet/') === -1) {
            $window.HTMLCanvasElement.prototype.toDataURL.toString = function () {
                return 'toDataURL() { [native code] }';
            };
            $window.HTMLCanvasElement.prototype.toDataURL.toString.toString = function () {
                return 'toString() { [native code] }';
            };
            $window.HTMLCanvasElement.prototype.toDataURL.toString.toString.toString = function () {
                return 'toString() { [native code] }';
            };
            $window.HTMLCanvasElement.prototype.toDataURL.toString.toString.toString.toString = function () {
                return 'toString() { [native code] }';
            };
            $window.HTMLCanvasElement.prototype.toDataURL.toString.toString.toString.toString.toString = function () {
                return 'toString() { [native code] }';
            };
            $window.HTMLCanvasElement.prototype.toDataURL.toString.toString.toString.toString.toString.toString = function () {
                return 'toString() { [native code] }';
            };
            // }

            console.info('canvas patched');
        }

        /**
         * @param {InjectorOption} config config
         */
        function iframeContentPatching(config) {
            var g = HTMLIFrameElement.prototype.__lookupGetter__('contentWindow'),
                h = HTMLIFrameElement.prototype.__lookupGetter__('contentDocument');
            Object.defineProperties(HTMLIFrameElement.prototype,
                {
                    contentWindow: {
                        get: function () {
                            var a = g.call(this);

                            try {
                                a.HTMLCanvasElement;
                            } catch (d) {
                                try {
                                    patchWindow(a, config);
                                } catch (e) {
                                }
                                return a;
                            }
                            protectedCanvasFingerPrint(a, config);
                            patchWindow(a, config);
                            pathPlugins(a);
                            // injectMediaDevices(config);
                            patchWebgl(window, config);
                            pathAudio(a, config);
                            //try {
                            //    pathFonts(a, config);
                            //} catch (e) {
                            //    addToConsole('contentWindow PathFonts', e);
                            //} 
                            return a;
                        }
                    },
                    contentDocument: {
                        get: function () {
                            var a = g.call(this);

                            try {
                                a.HTMLCanvasElement;
                            } catch (d) {
                                try {
                                    patchWindow(a, config);
                                } catch (e) {
                                }
                                return a;
                            }
                            protectedCanvasFingerPrint(a, config);
                            patchWindow(a, config);
                            pathPlugins(a);
                            // injectMediaDevices(config);
                            patchWebgl(window, config);
                            pathAudio(a, config);
                            //try {
                            //    pathFonts(a, config);
                            //} catch (e) {
                            //    addToConsole('contentDocument PathFonts', e);
                            //} 
                            return h.call(this);
                        }
                    }
                });
        }

        /**
         *
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function patchDnt($window, config) {
            if (fakeProfile.IsSendDoNotTrack) {
                Object.defineProperty(navigator, 'doNotTrack', {
                    value: 1, configurable: false, enumerable: true, writable: true
                });

                console.info('DNT patched');
            }
        }

        /**
         *
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function patchWebgl($window, config) {
            injectWebGl(config);
        }

        /**
         *
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function pathAudio($window, config) {
            Object.defineProperty($window.AudioContext.prototype, 'baseLatency', {
                value: fakeProfile.BaseLatency, configurable: false, enumerable: true, writable: true
            });
            // let canPlayAudio = $window.Audio.prototype.canPlayType;
            // Object.defineProperty($window.Audio.prototype, 'canPlayType', {
            //     value: function (type) {
            //         if (type == 'audio/mp4' || type == 'audio/mp4; codecs="mp4a.40.2"' || type == 'audio/aac') {
            //             return 'probably';
            //         }
            //         return canPlayAudio.call(this, type);
            //     }, configurable: false, enumerable: true, writable: true
            // });

            inject(config);
            //pathAudioContext($window.AudioContext, config);
            //pathAudioContext($window.OfflineAudioContext, config);
        }

        /**
         *
         * @param {Window} $window
         * @param {InjectorOption} config
         */
        function pathFonts($window, config) {
            'use strict';
            if (!fakeProfile.HideFonts) {
                return;
            }

            function defineobjectproperty(val, e, c, w) {
                // Makes an object describing a property
                return {
                    value: val,
                    enumerable: !!e,
                    configurable: true,
                    writable: true
                }
            }


            let originalStyleSetProperty = $window.CSSStyleDeclaration.prototype.setProperty;
            let originalSetAttrib = $window.Element.prototype.setAttribute;
            let originalNodeAppendChild = $window.Node.prototype.appendChild;


            let DEFAULT = 'auto';


            //let fontListToUse = [ 'Calibri', 'Cambria', 'Cambria Math', 'Candara', 'Comic Sans MS', 'Comic Sans MS Bold', 'Comic Sans', 'Consolas', 'Constantia', 'Corbel', 'Courier New', 'Caurier Regular', 'Ebrima', 'Fixedsys Regular', 'Franklin Gothic', 'Gabriola Regular', 'Gadugi', 'Georgia', 'HoloLens MDL2 Assets Regular', 'Impact Regular', 'Javanese Text Regular', 'Leelawadee UI', 'Lucida Console Regular', 'Lucida Sans Unicode Regular', 'Malgun Gothic', 'Microsoft Himalaya Regular', 'Microsoft JhengHei', 'Microsoft JhengHei UI', 'Microsoft PhangsPa', 'Microsoft Sans Serif Regular', 'Microsoft Tai Le', 'Microsoft YaHei', 'Microsoft YaHei UI', 'Microsoft Yi Baiti Regular', 'MingLiU_HKSCS-ExtB Regular', 'MingLiu-ExtB Regular', 'Modern Regular', 'Mongolia Baiti Regular', 'MS Gothic Regular', 'MS PGothic Regular', 'MS Sans Serif Regular', 'MS Serif Regular', 'MS UI Gothic Regular', 'MV Boli Regular', 'Myanmar Text', 'Nimarla UI', 'MV Boli Regular', 'Myanmar Tet', 'Nirmala UI', 'NSimSun Regular', 'Palatino Linotype', 'PMingLiU-ExtB Regular', 'Roman Regular', 'Script Regular', 'Segoe MDL2 Assets Regular', 'Segoe Print', 'Segoe Script', 'Segoe UI', 'Segoe UI Emoji Regular', 'Segoe UI Historic Regular', 'Segoe UI Symbol Regular', 'SimSun Regular', 'SimSun-ExtB Regular', 'Sitka Banner', 'Sitka Display', 'Sitka Heading', 'Sitka Small', 'Sitka Subheading', 'Sitka Text', 'Small Fonts Regular', 'Sylfaen Regular', 'Symbol Regular', 'System Bold', 'Tahoma', 'Terminal', 'Times New Roman', 'Trebuchet MS', 'Verdana', 'Webdings Regular', 'Wingdings Regular', 'Yu Gothic', 'Yu Gothic UI',  'Calibri', 'Calibri Light', 'Cambria', 'Cambria Math', 'Candara', 'Comic Sans MS', 'Consolas', 'Constantia', 'Corbel', 'Courier', 'Courier New', 'Ebrima', 'Fixedsys', 'Franklin Gothic Medium', 'Gabriola', 'Gadugi', 'Georgia', 'HoloLens MDL2 Assets', 'Impact', 'Javanese Text', 'Leelawadee UI', 'Leelawadee UI Semilight', 'Lucida Console', 'Lucida Sans Unicode', 'MS Gothic', 'MS PGothic', 'MS Sans Serif', 'MS Serif', 'MS UI Gothic', 'MV Boli', 'Malgun Gothic', 'Malgun Gothic Semilight', 'Marlett', 'Microsoft Himalaya', 'Microsoft JhengHei', 'Microsoft JhengHei Light', 'Microsoft JhengHei UI', 'Microsoft JhengHei UI Light', 'Microsoft New Tai Lue', 'Microsoft PhagsPa', 'Microsoft Sans Serif', 'Microsoft Tai Le', 'Microsoft YaHei', 'Microsoft YaHei Light', 'Microsoft YaHei UI', 'Microsoft YaHei UI Light', 'Microsoft Yi Baiti', 'MingLiU-ExtB', 'MingLiU_HKSCS-ExtB', 'Modern', 'Mongolian Baiti', 'Myanmar Text', 'NSimSun', 'Nirmala UI', 'Nirmala UI Semilight', 'PMingLiU-ExtB', 'Palatino Linotype', 'Roman', 'Script', 'Segoe MDL2 Assets', 'Segoe Print', 'Segoe Script', 'Segoe UI', 'Segoe UI Black', 'Segoe UI Emoji', 'Segoe UI Historic', 'Segoe UI Light', 'Segoe UI Semibold', 'Segoe UI Semilight', 'Segoe UI Symbol', 'SimSun', 'SimSun-ExtB', 'Sitka Banner', 'Sitka Display', 'Sitka Heading', 'Sitka Small', 'Sitka Subheading', 'Sitka Text', 'Small Fonts', 'Sylfaen', 'Symbol', 'System', 'Tahoma', 'Terminal', 'Times New Roman', 'Trebuchet MS', 'Verdana', 'Webdings', 'Wingdings', 'Yu Gothic', 'Yu Gothic Light', 'Yu Gothic Medium', 'Yu Gothic UI', 'Yu Gothic UI Light', 'Yu Gothic UI Semibold', 'Yu Gothic UI Semilight'].map(function (x) { return x.toLowerCase() });
            //let fontListToUse =
            //    "AIGDT;AMGDT;AcadEref;Adobe Arabic;Adobe Caslon Pro;Adobe Caslon Pro Bold;Adobe Devanagari;Adobe Fan Heiti Std B;Adobe Fangsong Std R;Adobe Garamond Pro;Adobe Garamond Pro Bold;Adobe Gothic Std B;Adobe Hebrew;Adobe Heiti Std R;Adobe Kaiti Std R;Adobe Ming Std L;Adobe Myungjo Std M;Adobe Naskh Medium;Adobe Song Std L;Agency FB;Aharoni;Alexandra Script;Algerian;Amadeus;AmdtSymbols;AnastasiaScript;Andalus;Angsana New;AngsanaUPC;Annabelle;Aparajita;Arabic Transparent;Arabic Typesetting;Arial Baltic;Arial Black;Arial CE;Arial CYR;Arial Cyr;Arial Greek;Arial Narrow;Arial Rounded MT Bold;Arial TUR;Arial Unicode MS;Ariston;Arno Pro;Arno Pro Caption;Arno Pro Display;Arno Pro Light Display;Arno Pro SmText;Arno Pro Smbd;Arno Pro Smbd Caption;Arno Pro Smbd Display;Arno Pro Smbd SmText;Arno Pro Smbd Subhead;Arno Pro Subhead;BankGothic Lt BT;BankGothic Md BT;Baskerville Old Face;Batang;BatangChe;Bauhaus 93;Bell Gothic Std Black;Bell Gothic Std Light;Bell MT;Berlin Sans FB;Berlin Sans FB Demi;Bernard MT Condensed;Bickham Script One;Bickham Script Pro Regular;Bickham Script Pro Semibold;Bickham Script Two;Birch Std;Blackadder ITC;Blackoak Std;Bodoni MT;Bodoni MT Black;Bodoni MT Condensed;Bodoni MT Poster Compressed;Book Antiqua;Bookman Old Style;Bookshelf Symbol 7;Bradley Hand ITC;Britannic Bold;Broadway;Browallia New;BrowalliaUPC;Brush Script MT;Brush Script Std;Calibri;Calibri Light;Californian FB;Calisto MT;Calligraph;Cambria;Cambria Math;Candara;Carolina;Castellar;Centaur;Century;Century Gothic;Century Schoolbook;Ceremonious Two;Chaparral Pro;Chaparral Pro Light;Charlemagne Std;Chiller;CityBlueprint;Clarendon BT;Clarendon Blk BT;Clarendon Lt BT;Colonna MT;Comic Sans MS;CommercialPi BT;CommercialScript BT;Complex;Consolas;Constantia;Cooper Black;Cooper Std Black;Copperplate Gothic Bold;Copperplate Gothic Light;Copyist;Corbel;Cordia New;CordiaUPC;CountryBlueprint;Courier;Courier New;Courier New Baltic;Courier New CE;Courier New CYR;Courier New Cyr;Courier New Greek;Courier New TUR;Curlz MT;DFKai-SB;DaunPenh;David;Decor;DejaVu Sans;DejaVu Sans Condensed;DejaVu Sans Light;DejaVu Sans Mono;DejaVu Serif;DejaVu Serif Condensed;DilleniaUPC;DokChampa;Dotum;DotumChe;Dutch801 Rm BT;Dutch801 XBd BT;Ebrima;Eccentric Std;Edwardian Script ITC;Elephant;Engravers MT;Eras Bold ITC;Eras Demi ITC;Eras Light ITC;Eras Medium ITC;Estrangelo Edessa;EucrosiaUPC;Euphemia;EuroRoman;Eurostile;FangSong;Felix Titling;Fixedsys;Footlight MT Light;Forte;FrankRuehl;Franklin Gothic Book;Franklin Gothic Demi;Franklin Gothic Demi Cond;Franklin Gothic Heavy;Franklin Gothic Medium;Franklin Gothic Medium Cond;Freehand521 BT;FreesiaUPC;Freestyle Script;French Script MT;Futura Md BT;GDT;GENISO;Gabriola;Gadugi;Garamond;Garamond Premr Pro;Garamond Premr Pro Smbd;Gautami;Gentium Basic;Gentium Book Basic;Georgia;Giddyup Std;Gigi;Gill Sans MT;Gill Sans MT Condensed;Gill Sans MT Ext Condensed Bold;Gill Sans Ultra Bold;Gill Sans Ultra Bold Condensed;Gisha;Gloucester MT Extra Condensed;GothicE;GothicG;GothicI;Goudy Old Style;Goudy Stout;GreekC;GreekS;Gulim;GulimChe;Gungsuh;GungsuhChe;Haettenschweiler;Harlow Solid Italic;Harrington;Heather Script One;Helvetica;High Tower Text;Hobo Std;ISOCP;ISOCP2;ISOCP3;ISOCPEUR;ISOCT;ISOCT2;ISOCT3;ISOCTEUR;Impact;Imprint MT Shadow;Informal Roman;IrisUPC;Iskoola Pota;Italic;ItalicC;ItalicT;JasmineUPC;Jokerman;Juice ITC;KaiTi;Kalinga;Kartika;Khmer UI;KodchiangUPC;Kokila;Kozuka Gothic Pr6N B;Kozuka Gothic Pr6N EL;Kozuka Gothic Pr6N H;Kozuka Gothic Pr6N L;Kozuka Gothic Pr6N M;Kozuka Gothic Pr6N R;Kozuka Gothic Pro B;Kozuka Gothic Pro EL;Kozuka Gothic Pro H;Kozuka Gothic Pro L;Kozuka Gothic Pro M;Kozuka Gothic Pro R;Kozuka Mincho Pr6N B;Kozuka Mincho Pr6N EL;Kozuka Mincho Pr6N H;Kozuka Mincho Pr6N L;Kozuka Mincho Pr6N M;Kozuka Mincho Pr6N R;Kozuka Mincho Pro B;Kozuka Mincho Pro EL;Kozuka Mincho Pro H;Kozuka Mincho Pro L;Kozuka Mincho Pro M;Kozuka Mincho Pro R;Kristen ITC;Kunstler Script;Lao UI;Latha;Leelawadee;Letter Gothic Std;Levenim MT;Liberation Sans Narrow;LilyUPC;Lithos Pro Regular;Lucida Bright;Lucida Calligraphy;Lucida Console;Lucida Fax;Lucida Handwriting;Lucida Sans;Lucida Sans Typewriter;Lucida Sans Unicode;MS Gothic;MS Mincho;MS Outlook;MS PGothic;MS PMincho;MS Reference Sans Serif;MS Reference Specialty;MS Sans Serif;MS Serif;MS UI Gothic;MT Extra;MV Boli;Magneto;Maiandra GD;Malgun Gothic;Mangal;Marlett;Matura MT Script Capitals;Meiryo;Meiryo UI;Mesquite Std;Microsoft Himalaya;Microsoft JhengHei;Microsoft JhengHei UI;Microsoft New Tai Lue;Microsoft PhagsPa;Microsoft Sans Serif;Microsoft Tai Le;Microsoft Uighur;Microsoft YaHei;Microsoft YaHei UI;Microsoft Yi Baiti;MingLiU;MingLiU-ExtB;MingLiU_HKSCS;MingLiU_HKSCS-ExtB;Minion Pro;Minion Pro Cond;Minion Pro Med;Minion Pro SmBd;Miriam;Miriam Fixed;Mistral;Modern;Modern No. 20;Mongolian Baiti;Monospac821 BT;Monotxt;Monotype Corsiva;MoolBoran;Myriad Arabic;Myriad Hebrew;Myriad Pro;Myriad Pro Cond;Myriad Pro Light;Myriad Web Pro;NSimSun;Narkisim;Niagara Engraved;Niagara Solid;Nirmala UI;Nueva Std;Nueva Std Cond;Nyala;OCR A Extended;OCR A Std;OCR-A BT;OCR-B 10 BT;Old English Text MT;Onyx;OpenSymbol;Orator Std;Ouverture script;PMingLiU;PMingLiU-ExtB;Palace Script MT;Palatino Linotype;PanRoman;Papyrus;Parchment;Perpetua;Perpetua Titling MT;Plantagenet Cherokee;Playbill;Poor Richard;Poplar Std;Prestige Elite Std;Pristina;Proxy 1;Proxy 2;Proxy 3;Proxy 4;Proxy 5;Proxy 6;Proxy 7;Proxy 8;Proxy 9;Raavi;Rage Italic;Ravie;Rockwell;Rockwell Condensed;Rockwell Extra Bold;Rod;Roman;RomanC;RomanD;RomanS;RomanT;Romantic;Rosewood Std Regular;Sakkal Majalla;SansSerif;Script;Script MT Bold;ScriptC;ScriptS;Segoe Print;Segoe Script;Segoe UI;Segoe UI Light;Segoe UI Semibold;Segoe UI Semilight;Segoe UI Symbol;Shonar Bangla;Showcard Gothic;Shruti;SimHei;SimSun;SimSun-ExtB;Simplex;Simplified Arabic;Simplified Arabic Fixed;Small Fonts;Snap ITC;Square721 BT;Stencil;Stencil Std;Stylus BT;SuperFrench;Swis721 BT;Swis721 BdCnOul BT;Swis721 BdOul BT;Swis721 Blk BT;Swis721 BlkCn BT;Swis721 BlkEx BT;Swis721 BlkOul BT;Swis721 Cn BT;Swis721 Ex BT;Swis721 Hv BT;Swis721 Lt BT;Swis721 LtCn BT;Swis721 LtEx BT;Syastro;Sylfaen;Symap;Symath;Symbol;Symeteo;Symusic;System;Tahoma;TeamViewer8;Technic;TechnicBold;TechnicLite;Tekton Pro;Tekton Pro Cond;Tekton Pro Ext;Tempus Sans ITC;Terminal;Times New Roman;Times New Roman Baltic;Times New Roman CE;Times New Roman CYR;Times New Roman Cyr;Times New Roman Greek;Times New Roman TUR;Traditional Arabic;Trajan Pro;Trebuchet MS;Tunga;Tw Cen MT;Tw Cen MT Condensed;Tw Cen MT Condensed Extra Bold;Txt;UniversalMath1 BT;Utsaah;Vani;Verdana;Vijaya;Viner Hand ITC;Vineta BT;Vivaldi;Vladimir Script;Vrinda;WP Arabic Sihafa;WP ArabicScript Sihafa;WP CyrillicA;WP CyrillicB;WP Greek Century;WP Greek Courier;WP Greek Helve;WP Hebrew David;WP MultinationalA Courier;WP MultinationalA Helve;WP MultinationalA Roman;WP MultinationalB Courier;WP MultinationalB Helve;WP MultinationalB Roman;WST_Czec;WST_Engl;WST_Fren;WST_Germ;WST_Ital;WST_Span;WST_Swed;Webdings;Wide Latin;Wingdings;Wingdings 2;Wingdings 3;ZWAdobeF"
            //        .split(';').map(function (x) { return x.toLowerCase() });;

            let fontListToUse = fakeProfile.Fonts.map(function (x) {
                return x.toLowerCase()
            });

            let baseFonts = ['auto'];
            let keywords = ['inherit', 'auto'];
            baseFonts.push.apply(baseFonts, fontListToUse);
            baseFonts.push.apply(baseFonts, keywords);


            function getAllowedFontFamily(family) {
                var fonts = family.replace(/"|'/g, '').split(',');

                var allowedFonts = fonts.filter(function (font) {
                    if (font && font.length) {
                        var normalised = font.trim().toLowerCase();
                        // Allow base fonts
                        for (var allowed of baseFonts)
                            if (normalised === allowed)
                                return true;

                        // Allow web fonts
                        for (var allowed1 of document.fonts.values())
                            if (normalised === allowed1)
                                return true;
                    }
                    return false;
                });

                if (allowedFonts.length === 0)
                    return undefined;
                return allowedFonts.map(function (f) {
                    var trimmed = f.trim();
                    return ~trimmed.indexOf(' ') ? "'" + trimmed + "'" : trimmed;
                }).join(', ');
            }


            function modifiedCssSetProperty(key, val) {
                if (key.toLowerCase() === 'font-family') {
                    let allowed = getAllowedFontFamily(val);
                    return originalStyleSetProperty.call(this, 'font-family', allowed || DEFAULT);
                }
                return originalStyleSetProperty.call(this, key, val);
            }

            function makeModifiedSetCssText(originalSetCssText) {
                return function modifiedSetCssText(css) {
                    var fontFamilyMatch = css.match(/\b(?:font-family:([^;]+)(?:;|$))/i);
                    if (fontFamilyMatch && fontFamilyMatch.length == 1) {
                        css = css.replace(/\b(font-family:[^;]+(;|$))/i, '').trim();
                        var allowed = getAllowedFontFamily(fontFamilyMatch[1]) || DEFAULT;
                        if (css.length && css[css.length - 1] !== ';')
                            css += ';';
                        css += 'font-family: ' + allowed + ';';
                    }
                    return originalSetCssText.call(this, css);
                }
            }

            var modifiedSetAttribute = (function () {
                var innerModify = makeModifiedSetCssText(function (val) {
                    return originalSetAttrib.call(this, 'style', val);
                });
                return function modifiedSetAttribute(key, val) {
                    if (key.toLowerCase() === 'style') {
                        return innerModify.call(this, val);
                    }
                    return originalSetAttrib.call(this, key, val);
                }
            })();

            function makeModifiedInnerHtml(originalInnerHtml) {
                return function modifiedInnerHtml(html) {
                    //Add as normal, then fix before returning
                    var retval = originalInnerHtml.call(this, html);
                    recursivelyModifyFonts(this.parentNode);
                    return retval;
                }
            }

            function recursivelyModifyFonts(elem) {
                if (elem) {
                    if (elem.style && elem.style.fontFamily) {
                        modifiedCssSetProperty.call(elem.style, 'font-family', elem.style.fontFamily); // Uses the special setter
                    }
                    if (elem.childNodes)
                        elem.childNodes.forEach(recursivelyModifyFonts);
                }
                return elem;
            }

            function modifiedAppend(child) {
                child = recursivelyModifyFonts(child);
                return originalNodeAppendChild.call(this, child);
            }


            function overrideFunc(obj, name, f) {
                Object.defineProperty(obj.prototype, name, defineobjectproperty(f, true));
                obj.prototype[name].toString = function () {
                    return name + '() { [native code] }';
                }
                obj.prototype[name].toString.toString = function () {
                    return 'toString() { [native code] }';
                }
            }


            function overrideSetter(obj, name, makeSetter) {
                var current = Object.getOwnPropertyDescriptor(obj.prototype, name);
                current.set = makeSetter(current.set);
                current.configurable = true;
                Object.defineProperty(obj.prototype, name, current);
            }

            overrideFunc($window.Node, 'appendChild', modifiedAppend);
            overrideFunc($window.CSSStyleDeclaration, 'setProperty', modifiedCssSetProperty);
            overrideFunc($window.Element, 'setAttribute', modifiedSetAttribute);

            Object.defineProperty($window.CSSStyleDeclaration.prototype, 'fontFamily', {
                set: function fontFamily(f) {
                    modifiedCssSetProperty.call(this, 'font-family', f);
                },
                get: function fontFamily() {
                    return this.getPropertyValue('font-family');
                }, configurable: true
            });


            overrideSetter($window.CSSStyleDeclaration, 'cssText', makeModifiedSetCssText);
            overrideSetter($window.Element, 'innerHTML', makeModifiedInnerHtml);
            overrideSetter($window.Element, 'outerHTML', makeModifiedInnerHtml);
        }


    })();