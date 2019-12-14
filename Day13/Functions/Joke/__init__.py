import logging

import azure.functions as func
from textgenrnn import textgenrnn
import tempfile
import os
import urllib.request

def main(
    myblob: func.InputStream,
    context: func.Context):
    
    tempFilePath = tempfile.gettempdir()
    jokesfile = os.path.join(tempFilePath,"jokes.hdf5")
    
    urllib.request.urlretrieve(myblob.uri, jokesfile)
    
    textgen = textgenrnn(jokesfile)
    joke = textgen.generate(return_as_list=True)[0]
    logging.info(f"joke: {joke}")
