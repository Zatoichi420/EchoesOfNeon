// Echoes of Neon: iOS speech for players without VoiceOver, and an audio session
// that keeps the game audible with the ring/silent switch on silent.
// Called from SpeechFallback.cs (DllImport "__Internal").

#import <AVFoundation/AVFoundation.h>

static AVSpeechSynthesizer *gSynth;

// Unity's default session (Ambient) is silenced by the ring/silent switch. An audio game
// must never go quiet that way, so use Playback, mixing with other apps' audio.
static void EchoSpeech_ConfigureSession(void)
{
    NSError *error = nil;
    [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback
                                     withOptions:AVAudioSessionCategoryOptionMixWithOthers
                                           error:&error];
    if (error) NSLog(@"[EchoSpeech] audio session category failed: %@", error);
    [[AVAudioSession sharedInstance] setActive:YES error:nil];
}

// Called by SpeechFallback once Unity has finished setting up its own audio
// (which would otherwise put the session back to Ambient), and before each line.
extern "C" void EchoSpeech_KeepAudible(void)
{
    if (![[AVAudioSession sharedInstance].category isEqualToString:AVAudioSessionCategoryPlayback])
        EchoSpeech_ConfigureSession();
}

extern "C" void EchoSpeech_Speak(const char *text)
{
    if (text == NULL) return;
    NSString *line = [NSString stringWithUTF8String:text];
    if (gSynth == nil)
    {
        gSynth = [[AVSpeechSynthesizer alloc] init];
        gSynth.usesApplicationAudioSession = YES;
    }
    EchoSpeech_KeepAudible();
    // New line interrupts the old one, matching the Mac and Windows fallbacks.
    [gSynth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
    AVSpeechUtterance *utterance = [AVSpeechUtterance speechUtteranceWithString:line];
    utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:[AVSpeechSynthesisVoice currentLanguageCode]];
    [gSynth speakUtterance:utterance];
}

extern "C" void EchoSpeech_Stop(void)
{
    if (gSynth != nil) [gSynth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
}
